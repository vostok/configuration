using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private const int MaxTypeCacheSize = 10;
        private const int MaxSourceCacheSize = 10;

        private readonly bool throwExceptions;
        private readonly Action<Exception> onError;
        private readonly Dictionary<Type, IConfigurationSource> typeSources;
        private readonly ISettingsBinder settingsBinder;

        private readonly ConcurrentDictionary<Type, object> typeCache;
        private readonly ConcurrentQueue<Type> typeCacheQueue;
        private readonly ConcurrentDictionary<Type, IObservable<object>> typeWatchers;
        //private readonly ConcurrentDictionary<Type, IDisposable> typeWatchers;
        //private readonly ConcurrentDictionary<Type, IList<IObserver<object>>> typeObservers;
        //private readonly ConcurrentDictionary<Type, BehaviorSubject<object>> typeObservers;

        private readonly ConcurrentDictionary<IConfigurationSource, object> sourceCache;
        private readonly ConcurrentQueue<IConfigurationSource> sourceCacheQueue;

        //todo config
        /// <summary>
        /// Creates a <see cref="ConfigurationProvider"/> instance with given parameters <paramref name="settingsBinder"/>, <paramref name="throwExceptions"/>, and <paramref name="onError"/>
        /// </summary>
        /// <param name="settingsBinder">Binder for using here</param>
        /// <param name="throwExceptions">Exception reaction</param>
        /// <param name="onError">Action on exception</param>
        public ConfigurationProvider(ISettingsBinder settingsBinder = null, bool throwExceptions = true, Action<Exception> onError = null)
        {
            this.throwExceptions = throwExceptions;
            this.onError = onError;
            this.settingsBinder = settingsBinder ?? new DefaultSettingsBinder();
            typeSources = new Dictionary<Type, IConfigurationSource>();
            typeWatchers = new ConcurrentDictionary<Type, IObservable<object>>();
            //typeWatchers = new ConcurrentDictionary<Type, IDisposable>();
            typeCache = new ConcurrentDictionary<Type, object>();
            typeCacheQueue = new ConcurrentQueue<Type>();
            //typeObservers = new ConcurrentDictionary<Type, IList<IObserver<object>>>();
            //typeObservers = new ConcurrentDictionary<Type, BehaviorSubject<object>>();
            sourceCache = new ConcurrentDictionary<IConfigurationSource, object>();
            sourceCacheQueue = new ConcurrentQueue<IConfigurationSource>();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Returns value of given type <typeparamref name="TSettings"/> using binder from constructor.</para>
        /// <para>Uses cache.</para>
        /// </summary>
        public TSettings Get<TSettings>()
        {
            var type = typeof(TSettings);
            if (typeCache.TryGetValue(type, out var item))
                return (TSettings)item;
            if (!typeSources.TryGetValue(type, out var source))
                throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{type.Name}\" is absent");
            return GetInternal<TSettings>(source.Get(), false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns value of given type <typeparamref name="TSettings"/> from specified <paramref name="source"/>.
        /// </summary>
        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            if (sourceCache.TryGetValue(source, out var item))
                return (TSettings)item;

            try
            {
                var value = settingsBinder.Bind<TSettings>(source.Get());
                sourceCache.TryAdd(source, value);
                sourceCacheQueue.Enqueue(source);
                if (sourceCache.Count > MaxSourceCacheSize && sourceCacheQueue.TryDequeue(out var src))
                    sourceCache.TryRemove(src, out var _);
                return value;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return default;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see changes in source.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new value</returns>
        public IObservable<TSettings> Observe<TSettings>()
        {
            var type = typeof(TSettings);
            if (typeWatchers.TryGetValue(type, out var watcher))
                return watcher.Select(s => (TSettings)s);
            typeWatchers[type] = typeSources[type].Observe().Select(settings => (object)GetInternal<TSettings>(settings, true));
            return typeWatchers[type].Select(s => (TSettings)s);
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see changes in specified <paramref name="source"/>.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new value</returns>
        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source) =>
            source.Observe().Select(settings => GetInternal<TSettings>(settings, false));

        /// <summary>
        /// Changes source to combination of source for given type <typeparamref name="TSettings"/> and <paramref name="source"/>
        /// </summary>
        /// <typeparam name="TSettings">Type of souce to combine with</typeparam>
        /// <param name="source">Second souce to combine with</param>
        public ConfigurationProvider SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            if (typeWatchers.Any())
                throw new InvalidOperationException($"It is not allowed to add sources to a {nameof(ConfigurationProvider)} after .{nameof(Observe)}() was called.");

            var type = typeof(TSettings);
            if (typeSources.TryGetValue(type, out var existingSource))
                source = existingSource.Combine(source);
            typeSources[type] = source;

            return this;
        }

        internal bool IsInCache(IConfigurationSource source, object value) =>
            sourceCache.TryGetValue(source, out var x) && (Equals(x, value) || value == null);

        internal bool IsInCache(Type type, object value) =>
            typeCache.TryGetValue(type, out var x) && (Equals(x, value) || value == null);

        private TSettings GetInternal<TSettings>(IRawSettings settings, bool getCached = false)
        {
            // CR(iloktionov): A bug here. Suppose I set up some source for MySettings model and the perform an ordinary Get().
            // CR(iloktionov): Result gets cached and then it's impossible to get MySettings with any other source.
            var type = typeof(TSettings);
            if (getCached && typeCache.TryGetValue(type, out var item))
                return (TSettings)item;

            try
            {
                var value = settingsBinder.Bind<TSettings>(settings);
                typeCache.TryAdd(type, value);
                typeCacheQueue.Enqueue(type);
                if (typeCache.Count > MaxTypeCacheSize && typeCacheQueue.TryDequeue(out var tp))
                    typeCache.TryRemove(tp, out var _);
                return value;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return default;
            }
        }
    }
}