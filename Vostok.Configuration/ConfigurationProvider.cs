using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Cache;
using Vostok.Configuration.CurrentValueProvider;
using Vostok.Configuration.Extensions;
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
        private readonly AtomicBoolean setupDisabled = new AtomicBoolean(false);
        private readonly Action<Exception> errorCallback;
        private readonly Action<object, IConfigurationSource> settingsCallback;
        private readonly IObservableBinder observableBinder;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ICurrentValueProviderFactory currentValueProviderFactory;
        private readonly TimeSpan sourceRetryCooldown;

        private readonly EventLoopScheduler baseScheduler;
        private readonly IScheduler scheduler;

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
                settings.SettingsCallback,
                new ObservableBinder(new CachingBinder(new ValidatingBinder(settings.Binder ?? new DefaultSettingsBinder()))),
                new SourceDataCache(settings.MaxSourceCacheSize),
                new RetryingCurrentValueProviderFactory(settings.ValueRetryCooldown, DecorateErrorCallback(settings.ErrorCallback)),
                settings.SourceRetryCooldown)
        {
        }

        internal ConfigurationProvider(
            Action<Exception> errorCallback,
            Action<object, IConfigurationSource> settingsCallback,
            IObservableBinder observableBinder,
            ISourceDataCache sourceDataCache,
            ICurrentValueProviderFactory currentValueProviderFactory,
            TimeSpan sourceRetryCooldown = default)
        {
            this.errorCallback = DecorateErrorCallback(errorCallback);
            this.settingsCallback = settingsCallback ?? ((_, __) => {});
            this.observableBinder = observableBinder;
            this.sourceDataCache = sourceDataCache;
            this.currentValueProviderFactory = currentValueProviderFactory;
            this.sourceRetryCooldown = sourceRetryCooldown;

            baseScheduler = new EventLoopScheduler();
            scheduler = baseScheduler.Catch<Exception>(
                exception =>
                {
                    this.errorCallback(exception);
                    return true;
                });
        }

        /// <inheritdoc />
        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>(out var source);
            DisableSetupSource();

            var cacheItem = sourceDataCache.GetPersistentCacheItem<TSettings>(source);
            if (cacheItem.CurrentValueProvider == null)
                cacheItem.TrySetCurrentValueProvider(currentValueProviderFactory.Create(ObserveWithErrors<TSettings>));

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

            var currentValueProvider = currentValueProviderFactory.Create(() => ObserveWithErrors<TSettings>(source));
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
        public void SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            if (setupDisabled)
                throw new InvalidOperationException($"Cannot set up source after {nameof(Get)}() or {nameof(Observe)}() was called.");

            typeSources[typeof(TSettings)] = source;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            baseScheduler.Dispose();
            sourceDataCache.Dispose();
        }

        internal IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>()
        {
            EnsureSourceExists<TSettings>(out var source);
            DisableSetupSource();

            return observableBinder
                .SelectBound(PushAndResubscribeOnErrors(source).ObserveOn(scheduler), () => sourceDataCache.GetPersistentCacheItem<TSettings>(source))
                .Do(newValue => OnSettingsInstance(source, newValue));
        }

        internal IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return ObserveWithErrors<TSettings>();

            return observableBinder
                .SelectBound(PushAndResubscribeOnErrors(source).ObserveOn(scheduler), () => sourceDataCache.GetLimitedCacheItem<TSettings>(source))
                .Do(newValue => OnSettingsInstance(source, newValue));
        }

        private void DisableSetupSource()
        {
            if (setupDisabled)
                return;

            setupDisabled.Value = true;
        }

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

        private void OnSettingsInstance<T>(IConfigurationSource source, (T settings, Exception error) newValue)
        {
            if (newValue.error != null || newValue.settings == null)
                return;

            settingsCallback(newValue.settings, source);
        }

        [NotNull]
        private static Action<Exception> DecorateErrorCallback([CanBeNull] Action<Exception> userCallback)
        {
            return exception =>
            {
                try
                {
                    userCallback?.Invoke(exception);
                }
                catch
                {
                    // ignored
                }
            };
        }
    }
}
