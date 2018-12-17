using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfigurationGetter configurationGetter;
        private readonly IConfigurationObservable configurationObservable;
        private readonly IConfigurationWithErrorsObservable configurationWithErrorsObservable;
        private static readonly string UnknownTypeExceptionMsg = $"{nameof(IConfigurationSource)} for specified type \"typeName\" is absent. Use {nameof(SetupSourceFor)} to add source.";
        private readonly ConfigurationProviderSettings settings;

        private readonly ConcurrentDictionary<Type, IConfigurationSource> typeSources = new ConcurrentDictionary<Type, IConfigurationSource>();
        private readonly ConcurrentDictionary<Type, bool> setupDisabled = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        ///     Creates a <see cref="ConfigurationProvider" /> instance with given settings
        ///     <paramref name="configurationProviderSettings" />
        /// </summary>
        /// <param name="configurationProviderSettings">
        ///     Provider settings. Uses <see cref="DefaultSettingsBinder" /> if
        ///     <see cref="ConfigurationProviderSettings.Binder" /> is null.
        /// </param>
        public ConfigurationProvider(ConfigurationProviderSettings configurationProviderSettings = null)
        {
            settings = configurationProviderSettings ?? new ConfigurationProviderSettings();
            
            var cachingBinder = new CachingBinder(new ValidatingBinder(settings.Binder ?? new DefaultSettingsBinder()));
            var observableBinder = new ObservableBinder(cachingBinder);
            var sourceDataCache = new SourceDataCache(settings.MaxSourceCacheSize);
            var taskSourceFactory = new TaskSourceFactory();
            
            configurationWithErrorsObservable = new ConfigurationWithErrorsObservable(GetSource, observableBinder, sourceDataCache);
            configurationObservable = new ConfigurationObservable(configurationWithErrorsObservable, settings.ErrorCallBack);
            configurationGetter = new ConfigurationGetter(GetSource, configurationObservable, sourceDataCache, taskSourceFactory);
        }

        internal ConfigurationProvider(IConfigurationGetter configurationGetter, IConfigurationObservable configurationObservable, IConfigurationWithErrorsObservable configurationWithErrorsObservable)
        {
            this.configurationGetter = configurationGetter;
            this.configurationObservable = configurationObservable;
            this.configurationWithErrorsObservable = configurationWithErrorsObservable;
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Returns value of given type <typeparamref name="TSettings" /> using binder from constructor.</para>
        ///     <para>Uses cache.</para>
        /// </summary>
        public TSettings Get<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();
            return configurationGetter.Get<TSettings>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns value of given type <typeparamref name="TSettings" /> from specified <paramref name="source" />.
        /// </summary>
        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            if (IsConfiguredFor<TSettings>(source))
                return Get<TSettings>();

            return configurationGetter.Get<TSettings>(source);
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Subscribtion to see changes in source.</para>
        ///     <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new value</returns>
        public IObservable<TSettings> Observe<TSettings>()
        {
            EnsureSourceExists<TSettings>();
            DisableSetupSourceFor<TSettings>();
            return configurationObservable.Observe<TSettings>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Subscribtion to see changes in specified <paramref name="source" />.</para>
        ///     <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new value</returns>
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

        /// <summary>
        ///     Changes source to combination of source for given type <typeparamref name="TSettings" /> and
        ///     <paramref name="source" />
        /// </summary>
        /// <typeparam name="TSettings">Type of souce to combine with</typeparam>
        /// <param name="source">Second souce to combine with</param>
        public void SetupSourceFor<TSettings>(IConfigurationSource source)
        {
            var type = typeof(TSettings);
            if (setupDisabled.ContainsKey(type))
                throw new InvalidOperationException($"{nameof(ConfigurationProvider)}: it is not allowed to add sources for \"{type.Name}\" to a {nameof(ConfigurationProvider)} after {nameof(Get)}() or {nameof(Observe)}() was called for this type.");
            
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
                throw new ArgumentException($"{UnknownTypeExceptionMsg.Replace("typeName", type.Name)}");
        }

        private bool IsConfiguredFor<TSettings>(IConfigurationSource source)
        {
            var type = typeof(TSettings);
            return typeSources.ContainsKey(type) && ReferenceEquals(typeSources[type], source);
        }

        private IConfigurationSource GetSource(Type type)
        {
            EnsureSourceExists(type);
            return typeSources[type];
        }
    }
}