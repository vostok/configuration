using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Cache;
using Vostok.Configuration.ProviderComponents;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfigurationGetter configurationGetter;
        private readonly IConfigurationObservable configurationObservable;
        private readonly IConfigurationWithErrorsObservable configurationWithErrorsObservable;
        private readonly ConfigurationProviderSettings settings;

        private readonly ConcurrentDictionary<Type, IConfigurationSource> typeSources = new ConcurrentDictionary<Type, IConfigurationSource>();
        private readonly ConcurrentDictionary<Type, bool> setupDisabled = new ConcurrentDictionary<Type, bool>();

        public ConfigurationProvider()
            : this(new ConfigurationProviderSettings())
        {
        }

        public ConfigurationProvider(ConfigurationProviderSettings settings)
        {
            this.settings = settings;

            var cachingBinder = new CachingBinder(new ValidatingBinder(settings.Binder ?? new DefaultSettingsBinder()));
            var observableBinder = new ObservableBinder(cachingBinder);
            var sourceDataCache = new SourceDataCache(settings.MaxSourceCacheSize);
            var taskSourceFactory = new TaskSourceFactory();

            configurationWithErrorsObservable = new ConfigurationWithErrorsObservable(GetSource, observableBinder, sourceDataCache);
            configurationObservable = new ConfigurationObservable(configurationWithErrorsObservable, settings.ErrorCallback);
            configurationGetter = new ConfigurationGetter(GetSource, configurationObservable, sourceDataCache, taskSourceFactory);
        }

        internal ConfigurationProvider(IConfigurationGetter configurationGetter, IConfigurationObservable configurationObservable, IConfigurationWithErrorsObservable configurationWithErrorsObservable)
        {
            this.configurationGetter = configurationGetter;
            this.configurationObservable = configurationObservable;
            this.configurationWithErrorsObservable = configurationWithErrorsObservable;
        }

        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();
            return configurationGetter.Get<TSettings>();
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return Get<TSettings>();

            return configurationGetter.Get<TSettings>(source);
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();
            return configurationObservable.Observe<TSettings>();
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return Observe<TSettings>();

            return configurationObservable.Observe<TSettings>(source);
        }

        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();
            return configurationWithErrorsObservable.ObserveWithErrors<TSettings>();
        }

        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return ObserveWithErrors<TSettings>();

            return configurationWithErrorsObservable.ObserveWithErrors<TSettings>(source);
        }

        public void SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            var type = typeof(TSettings);
            if (setupDisabled.ContainsKey(type))
                throw new InvalidOperationException($"Cannot set up source for type '{type}' after {nameof(Get)}() or {nameof(Observe)}() was called for this type.");

            typeSources[type] = source;
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
                throw new ArgumentException($"There is no preconfigured source for settings of type '{type}'. Use {nameof(SetupSourceFor)} to configure it.");
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
        {
            return typeSources.TryGetValue(typeof(TSettings), out var preconfiguredSource) && ReferenceEquals(source, preconfiguredSource);
        }

        private IConfigurationSource GetSource(Type type)
        {
            EnsureSourceExists(type);
            return typeSources[type];
        }
    }
}