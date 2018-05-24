using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly bool throwExceptions;
        private readonly Action<Exception> onError;
        private readonly Dictionary<Type, IConfigurationSource> typeSources;
        private readonly ISettingsBinder settingsBinder;
        private readonly TimeSpan cacheTime;

        private readonly ConcurrentDictionary<Type, object> typeCache;
        private readonly ConcurrentDictionary<Type, IDisposable> typeWatchers;
        private readonly ConcurrentDictionary<Type, BehaviorSubject<object>> typeObservers;

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
            this.cacheTime = cacheTime != default ? cacheTime : 10.Seconds();
            this.settingsBinder = settingsBinder ?? new DefaultSettingsBinder();
            typeSources = new Dictionary<Type, IConfigurationSource>();
            typeCache = new ConcurrentDictionary<Type, object>();
            typeWatchers = new ConcurrentDictionary<Type, IDisposable>();
            typeObservers = new ConcurrentDictionary<Type, BehaviorSubject<object>>();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Returns value of given type <typeparamref name="TSettings"/> using binder from constructor.</para>
        /// <para>Uses cache.</para>
        /// </summary>
        public TSettings Get<TSettings>()
        {
            try
            {
                if (typeCache.TryGetValue(typeof(TSettings), out var item))
                    return (TSettings)item;
                if (!typeSources.TryGetValue(typeof(TSettings), out var source))
                    throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");
                return GetInternal<TSettings>(source.Get(), false);
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
        /// Returns value of given type <typeparamref name="TSettings"/> from specified <paramref name="source"/>.
        /// </summary>
        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            try
            {
                //todo: one more cache for souces?
                return GetInternal<TSettings>(source.Get());
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return default;
            }
        }

        private TSettings GetInternal<TSettings>(IRawSettings settings, bool cached = false)
        {
            // CR(iloktionov): A bug here. Suppose I set up some source for MySettings model and the perform an ordinary Get().
            // CR(iloktionov): Result gets cached and then it's impossible to get MySettings with any other source.
            object item = default;
            if (cached && typeCache.TryGetValue(typeof(TSettings), out item))
                return (TSettings)item;

            try
            {
                var value = settingsBinder.Bind<TSettings>(settings);
                typeCache[typeof(TSettings)] = value;
                return value;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return (TSettings)item;
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
            if (!typeSources.TryGetValue(type, out var source))
                throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");

            var subject = null as BehaviorSubject<object>;
            subject = typeObservers.GetOrAdd(type, _ => subject = new BehaviorSubject<object>(null));
            
            return Observable.Create<TSettings>(observer =>
                subject.Select(obj => GetInternal<TSettings>(source.Get(), true)).Subscribe(observer));
        }
        /*{
            if (!sources.TryGetValue(typeof(TSettings), out var source))
                throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");

            var newSubject = null as BehaviorSubject<object>;
            var subject = typeObservers.GetOrAdd(typeof(TSettings), _ => newSubject = new BehaviorSubject<object>(null));

            if (subject == newSubject)
                source.Observe().Where(s => s != null).Select(settings => GetInternal<TSettings>(settings, true) as object).Subscribe(subject);

            return subject.Where(s => s != null).Cast<TSettings>();
        }*/

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see changes in specified <paramref name="source"/>.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new value</returns>
        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source) => 
            source.Observe().Select(settings => GetInternal<TSettings>(settings)).Where(s => s != null);

        /// <summary>
        /// Changes source to combination of source for given type <typeparamref name="TSettings"/> and <paramref name="source"/>
        /// </summary>
        /// <typeparam name="TSettings">Type of souce to combine with</typeparam>
        /// <param name="source">Second souce to combine with</param>
        public ConfigurationProvider SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            if (typeObservers.Any())
                throw new InvalidOperationException($"It is not allowed to add sources to a {nameof(ConfigurationProvider)} after .{nameof(Observe)}() was called.");

            var type = typeof(TSettings);
            if (typeSources.TryGetValue(type, out var existingSource))
            {
                source = existingSource.Combine(source);
                typeWatchers[type].Dispose();
            }

            typeSources[type] = source;
            typeWatchers[type] = source.Observe().Subscribe(settings => GetInternal<TSettings>(settings, false));

            return this;
        }
    }
}