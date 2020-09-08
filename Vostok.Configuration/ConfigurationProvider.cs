using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Rx;
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
        private readonly ConcurrentDictionary<Type, (IConfigurationSource source, bool used)> typeSources
            = new ConcurrentDictionary<Type, (IConfigurationSource source, bool used)>();

        private readonly Action<Exception> errorCallback;
        private readonly Action<object, IConfigurationSource> settingsCallback;
        private readonly IObservableBinder observableBinder;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ICurrentValueProviderFactory currentValueProviderFactory;
        private readonly TimeSpan sourceRetryCooldown;

        private readonly EventLoopScheduler baseScheduler;
        private readonly IScheduler scheduler;

        static ConfigurationProvider()
        {
            RxHacker.Hack();
        }

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
                new RetryingCurrentValueProviderFactory(settings.ValueRetryCooldown),
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
            this.errorCallback = new ErrorCallbackDecorator(errorCallback).Invoke;
            this.settingsCallback = new SettingsCallbackDecorator(settingsCallback, this.errorCallback).Invoke;
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

        /// <summary>
        /// <para>Returns a static shared instance of <see cref="ConfigurationProvider"/>.</para>
        /// <para>It's intended to be used by library developers to avoid lifetime management of private <see cref="ConfigurationProvider"/> objects and prevent resource leaks.</para>
        /// <para>The instance returned by this property can be substituted with <see cref="TrySetDefault"/> method.</para>
        /// </summary>
        [NotNull]
        public static ConfigurationProvider Default => DefaultConfigurationProvider.Get();

        /// <summary>
        /// Replaces the instance returned by <see cref="Default"/> property with given <paramref name="provider"/>.
        /// </summary>
        public static bool TrySetDefault([NotNull] ConfigurationProvider provider, bool canOverwrite = false)
            => DefaultConfigurationProvider.TryConfigure(provider, canOverwrite);

        /// <inheritdoc />
        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>(out var source);

            var cacheItem = sourceDataCache.GetPersistentCacheItem<TSettings>(source);
            if (cacheItem.CurrentValueProvider == null)
                cacheItem.TrySetCurrentValueProvider(currentValueProviderFactory.Create(ObserveWithErrors<TSettings>, errorCallback));

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

            var currentValueProvider = currentValueProviderFactory.Create(() => ObserveWithErrors<TSettings>(source), errorCallback);
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
            => SetupSourceFor(typeof(TSettings), source);

        /// <inheritdoc cref="SetupSourceFor{TSettings}"/>
        public void SetupSourceFor(Type settingsType, IConfigurationSource source)
        {
            while (true)
            {
                var alreadyConfigured = typeSources.TryGetValue(settingsType, out var result);
                if (alreadyConfigured)
                {
                    if (ReferenceEquals(source, result.source))
                        return;

                    if (result.used)
                        throw new InvalidOperationException($"Cannot set up source for type '{settingsType.Name}' after {nameof(Get)}() or {nameof(Observe)}() was called.");
                }

                var reconfigured = alreadyConfigured
                    ? typeSources.TryUpdate(settingsType, (source, false), result)
                    : typeSources.TryAdd(settingsType, (source, false));

                if (reconfigured)
                    return;
            }
        }

        /// <summary>
        /// <para>Attempts to set associate given <paramref name="source"/> with type <typeparamref name="TSettings"/>.</para>
        /// <para>Returns <c>true</c> on success or <c>false</c> if a source has already been configured for this type.</para>
        /// <para>See <see cref="SetupSourceFor"/> for mor details.</para>
        /// </summary>
        public bool TrySetupSourceFor<TSettings>([NotNull] IConfigurationSource source)
            => TrySetupSourceFor(typeof(TSettings), source);

        /// <summary>
        /// <para>Attempts to set associate given <paramref name="source"/> with type <paramref name="settingsType"/>.</para>
        /// <para>Returns <c>true</c> on success or <c>false</c> if another source has already been configured for this type.</para>
        /// <para>See <see cref="SetupSourceFor"/> for mor details.</para>
        /// </summary>
        public bool TrySetupSourceFor([NotNull] Type settingsType, [NotNull] IConfigurationSource source)
        {
            while (true)
            {
                if (typeSources.TryGetValue(settingsType, out var result))
                    return ReferenceEquals(source, result.source);

                if (typeSources.TryAdd(settingsType, (source, false)))
                    return true;
            }
        }

        /// <summary>
        /// Returns whether there is a source configured for <typeparamref name="TSettings"/>.
        /// </summary>
        public bool HasSourceFor<TSettings>() => HasSourceFor(typeof(TSettings));

        /// <summary>
        /// Returns whether there is a source configured for <paramref name="settingsType"/>.
        /// </summary>
        public bool HasSourceFor([NotNull] Type settingsType) => typeSources.ContainsKey(settingsType);

        /// <inheritdoc />
        public void Dispose()
        {
            baseScheduler.Dispose();
            sourceDataCache.Dispose();
        }

        internal IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>()
        {
            EnsureSourceExists<TSettings>(out var source);

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

        private void EnsureSourceExists<TSettings>(out IConfigurationSource source)
            => EnsureSourceExists(typeof(TSettings), out source);

        private void EnsureSourceExists(Type type, out IConfigurationSource source)
        {
            while (true)
            {
                if (!typeSources.TryGetValue(type, out var result))
                    throw new ArgumentException($"There is no preconfigured source for settings of type '{type}'. Use '{nameof(SetupSourceFor)}' method to configure it.");

                if (!result.used && !typeSources.TryUpdate(type, (result.source, true), result))
                    continue;

                source = result.source;
                return;
            }
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
            => typeSources.TryGetValue(typeof(TSettings), out var result) && ReferenceEquals(source, result.source);

        private IObservable<(ISettingsNode, Exception)> PushAndResubscribeOnErrors(IConfigurationSource source)
            => HealingObservable.PushAndResubscribeOnErrors(source.Observe, sourceRetryCooldown);

        private void OnSettingsInstance<T>(IConfigurationSource source, (T settings, Exception error) newValue)
        {
            if (newValue.error != null || newValue.settings == null)
                return;

            settingsCallback(newValue.settings, source);
        }
    }
}
