using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Cache;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.ObservableBinding;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration
{
    /// <summary>
    /// Use this class to obtain settings for your application.
    /// </summary>
    [PublicAPI]
    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        private readonly ConcurrentDictionary<Type, IConfigurationSource> typeSources = new ConcurrentDictionary<Type, IConfigurationSource>();
        private readonly ConcurrentDictionary<Type, bool> setupDisabled = new ConcurrentDictionary<Type, bool>();
        private readonly Action<Exception> errorCallback;
        private readonly IObservableBinder observableBinder;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ITaskSourceFactory taskSourceFactory;

        /// <summary>
        /// Create a new <see cref="ConfigurationProvider"/> instance.
        /// </summary>
        public ConfigurationProvider()
            : this(new ConfigurationProviderSettings())
        {
        }

        /// <inheritdoc cref="ConfigurationProvider()"/>
        public ConfigurationProvider(ConfigurationProviderSettings settings)
            : this(
                settings.ErrorCallback,
                new ObservableBinder(new CachingBinder(new ValidatingBinder(settings.Binder ?? new DefaultSettingsBinder()))),
                new SourceDataCache(settings.MaxSourceCacheSize),
                new TaskSourceFactory())
        {
        }

        internal ConfigurationProvider(Action<Exception> errorCallback, IObservableBinder observableBinder, ISourceDataCache sourceDataCache, ITaskSourceFactory taskSourceFactory)
        {
            this.errorCallback = errorCallback ?? (_ => {});
            this.observableBinder = observableBinder;
            this.sourceDataCache = sourceDataCache;
            this.taskSourceFactory = taskSourceFactory;
        }

        /// <inheritdoc />
        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();

            var source = GetSource<TSettings>();
            var cacheItem = sourceDataCache.GetPersistentCacheItem<TSettings>(source);
            if (cacheItem.TaskSource == null)
                cacheItem.TrySetTaskSource(taskSourceFactory.Create(Observe<TSettings>));

            return cacheItem.TaskSource.Get();
        }

        /// <inheritdoc />
        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return Get<TSettings>();

            var cacheItem = sourceDataCache.GetLimitedCacheItem<TSettings>(source);
            if (cacheItem.TaskSource != null)
                return cacheItem.TaskSource.Get();

            var taskSource = taskSourceFactory.Create(() => Observe<TSettings>(source));
            var result = taskSource.Get();
            if (!cacheItem.TrySetTaskSource(taskSource))
                taskSource.Dispose();

            return result;
        }

        /// <inheritdoc />
        public IObservable<TSettings> Observe<TSettings>()
        {
            return ObserveWithErrors<TSettings>().SendErrorsToCallback(errorCallback);
        }

        /// <inheritdoc />
        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            return ObserveWithErrors<TSettings>(source).SendErrorsToCallback(errorCallback);
        }

        /// <inheritdoc />
        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();

            var source = GetSource<TSettings>();

            return observableBinder.SelectBound(source.Observe(), () => sourceDataCache.GetPersistentCacheItem<TSettings>(source));
        }

        /// <inheritdoc />
        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return ObserveWithErrors<TSettings>();

            return observableBinder.SelectBound(source.Observe(), () => sourceDataCache.GetLimitedCacheItem<TSettings>(source));
        }

        /// <inheritdoc />
        public void SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            var type = typeof(TSettings);
            if (setupDisabled.ContainsKey(type))
                throw new InvalidOperationException($"Cannot set up source for type '{type}' after {nameof(Get)}() or {nameof(Observe)}() was called for this type.");

            typeSources[type] = source;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            sourceDataCache.Dispose();
        }

        private void DisableSetupSourceFor<TSettings>()
        {
            setupDisabled[typeof(TSettings)] = true;
        }

        private void EnsureSourceExists<TSettings>()
        {
            EnsureSourceExists(typeof(TSettings));
        }

        private void EnsureSourceExists(Type type)
        {
            if (!typeSources.ContainsKey(type))
                throw new ArgumentException($"There is no preconfigured source for settings of type '{type}'. Use '{nameof(SetupSourceFor)}' method to configure it.");
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
        {
            return typeSources.TryGetValue(typeof(TSettings), out var preconfiguredSource) && ReferenceEquals(source, preconfiguredSource);
        }

        private IConfigurationSource GetSource<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            return typeSources[typeof(TSettings)];
        }
    }
}