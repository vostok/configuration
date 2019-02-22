using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Cache;
using Vostok.Configuration.CurrentValueProvider;
using Vostok.Configuration.Helpers;
using Vostok.Configuration.ObservableBinding;

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
        private readonly EventLoopScheduler scheduler = new EventLoopScheduler();
        private readonly Action<Exception> errorCallback;
        private readonly IObservableBinder observableBinder;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ICurrentValueProviderFactory currentValueProviderFactory;
        private readonly TimeSpan sourceRetryCooldown;

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
                new RetryingCurrentValueProviderFactory(),
                settings.SourceRetryCooldown)
        {
        }

        internal ConfigurationProvider(
            Action<Exception> errorCallback,
            IObservableBinder observableBinder,
            ISourceDataCache sourceDataCache,
            ICurrentValueProviderFactory currentValueProviderFactory,
            TimeSpan sourceRetryCooldown = default)
        {
            this.errorCallback = errorCallback ?? (_ => {});
            this.observableBinder = observableBinder;
            this.sourceDataCache = sourceDataCache;
            this.currentValueProviderFactory = currentValueProviderFactory;
            this.sourceRetryCooldown = sourceRetryCooldown;
        }

        /// <inheritdoc />
        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>(out var source);
            DisableSetupSourceFor<TSettings>();

            var cacheItem = sourceDataCache.GetPersistentCacheItem<TSettings>(source);
            if (cacheItem.CurrentValueProvider == null)
                cacheItem.TrySetCurrentValueProvider(currentValueProviderFactory.Create(Observe<TSettings>));

            return cacheItem.CurrentValueProvider.Get();
        }

        /// <inheritdoc />
        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return Get<TSettings>();

            // There can be a race between CurrentValueProvider.Get() and CurrentValueProvider.Dispose() which is called when
            // CacheItem is evicted from cache.
            // We avoid it based on the following guarantees:
            // 1. Every CacheItem in cache has CurrentValueProvider which is successfully received at least one value.
            // 2. RawCurrentValueProvider does not replace existing value with an error.
            // 3. It's safe to use RawCurrentValueProvider which is received some value even after Dispose(), without leaking
            // the subscription.
            var cacheItem = sourceDataCache.GetLimitedCacheItem<TSettings>(source);
            if (cacheItem.CurrentValueProvider != null)
                return cacheItem.CurrentValueProvider.Get();

            var currentValueProvider = currentValueProviderFactory.Create(() => Observe<TSettings>(source));
            var result = currentValueProvider.Get();
            if (!cacheItem.TrySetCurrentValueProvider(currentValueProvider))
                currentValueProvider.Dispose();

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
            EnsureSourceExists<TSettings>(out var source);
            DisableSetupSourceFor<TSettings>();

            return observableBinder
                .SelectBound(PushAndResubscribeOnErrors(source), () => sourceDataCache.GetPersistentCacheItem<TSettings>(source))
                .ObserveOn(scheduler);
        }

        /// <inheritdoc />
        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return ObserveWithErrors<TSettings>();

            return observableBinder
                .SelectBound(PushAndResubscribeOnErrors(source), () => sourceDataCache.GetLimitedCacheItem<TSettings>(source))
                .ObserveOn(scheduler);
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
            scheduler.Dispose();
            sourceDataCache.Dispose();
        }

        private void DisableSetupSourceFor<TSettings>()
            => setupDisabled[typeof(TSettings)] = true;

        private void EnsureSourceExists<TSettings>(out IConfigurationSource source)
            => EnsureSourceExists(typeof(TSettings), out source);

        private void EnsureSourceExists(Type type, out IConfigurationSource source)
        {
            if (!typeSources.TryGetValue(type, out source))
                throw new ArgumentException($"There is no preconfigured source for settings of type '{type}'. Use '{nameof(SetupSourceFor)}' method to configure it.");
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
            => typeSources.TryGetValue(typeof(TSettings), out var preconfiguredSource) && ReferenceEquals(source, preconfiguredSource);

        private IObservable<(ISettingsNode, Exception)> PushAndResubscribeOnErrors(IConfigurationSource source)
            => HealingObservable.PushAndResubscribeOnErrors(source.Observe, sourceRetryCooldown);
    }
}