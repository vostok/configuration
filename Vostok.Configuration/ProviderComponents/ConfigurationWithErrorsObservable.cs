using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Cache;

namespace Vostok.Configuration.ProviderComponents
{
    internal class ConfigurationWithErrorsObservable : IConfigurationWithErrorsObservable
    {
        private readonly Func<Type, IConfigurationSource> sourceProvider;
        private readonly IObservableBinder observableBinder;
        private readonly ISourceDataCache sourceDataCache;

        public ConfigurationWithErrorsObservable(Func<Type, IConfigurationSource> sourceProvider, IObservableBinder observableBinder, ISourceDataCache sourceDataCache)
        {
            this.sourceProvider = sourceProvider;
            this.observableBinder = observableBinder;
            this.sourceDataCache = sourceDataCache;
        }

        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>()
        {
            var source = sourceProvider(typeof(TSettings));

            return observableBinder.SelectBound(source.Observe(), () => sourceDataCache.GetPersistentCacheItem<TSettings>(source));
        }

        public IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>(IConfigurationSource source)
        {
            return observableBinder.SelectBound(source.Observe(), () => sourceDataCache.GetLimitedCacheItem<TSettings>(source));
        }
    }
}