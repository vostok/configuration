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
        private ISettingsBinder settingsBinder;

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
                new ObservableBinder(),
                settings.Binder ?? new DefaultSettingsBinder(),
                new SourceDataCache(settings.MaxSourceCacheSize),
                new RetryingCurrentValueProviderFactory(),
                settings.SourceRetryCooldown)
        {
        }

        internal ConfigurationProvider(
            Action<Exception> errorCallback,
            IObservableBinder observableBinder,
            ISettingsBinder settingsBinder,
            ISourceDataCache sourceDataCache,
            ICurrentValueProviderFactory currentValueProviderFactory,
            TimeSpan sourceRetryCooldown = default)
        {
            this.errorCallback = errorCallback ?? (_ => {});
            this.observableBinder = observableBinder;
            Binder = settingsBinder;
            this.sourceDataCache = sourceDataCache;
            this.currentValueProviderFactory = currentValueProviderFactory;
            this.sourceRetryCooldown = sourceRetryCooldown;
        }

        /// <summary>
        /// <para>Use this to specify a custom implementation of <see cref="ISettingsBinder"/>.</para>
        /// <para><see cref="DefaultSettingsBinder"/> will be used by default.</para>
        /// </summary>
        public ISettingsBinder Binder
        {
            get => settingsBinder;
            set => observableBinder.Binder = new CachingBinder(new ValidatingBinder(settingsBinder = value));
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

            var cacheItem = sourceDataCache.GetLimitedCacheItem<TSettings>(source);
            if (cacheItem.CurrentValueProvider != null)
                return cacheItem.CurrentValueProvider.Get();

            // TODO(krait): comment
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

            return observableBinder.SelectBound(SubscribeToSource(source), () => sourceDataCache.GetPersistentCacheItem<TSettings>(source)).ObserveOn(scheduler);
        }

        /// <inheritdoc />
        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return ObserveWithErrors<TSettings>();

            return observableBinder.SelectBound(SubscribeToSource(source), () => sourceDataCache.GetLimitedCacheItem<TSettings>(source)).ObserveOn(scheduler);
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
        {
            setupDisabled[typeof(TSettings)] = true;
        }

        private void EnsureSourceExists<TSettings>(out IConfigurationSource source)
        {
            EnsureSourceExists(typeof(TSettings), out source);
        }

        private void EnsureSourceExists(Type type, out IConfigurationSource source)
        {
            if (!typeSources.TryGetValue(type, out source))
                throw new ArgumentException($"There is no preconfigured source for settings of type '{type}'. Use '{nameof(SetupSourceFor)}' method to configure it.");
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
        {
            return typeSources.TryGetValue(typeof(TSettings), out var preconfiguredSource) && ReferenceEquals(source, preconfiguredSource);
        }

        private IObservable<(ISettingsNode, Exception)> SubscribeToSource(IConfigurationSource source)
        {
            return HealingObservable.PushErrors(source.Observe, sourceRetryCooldown);
        }
    }
}