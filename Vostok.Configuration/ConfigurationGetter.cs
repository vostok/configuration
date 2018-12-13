using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration
{
    internal class ConfigurationGetter : IConfigurationGetter
    {
        private readonly IConfigurationObservable configurationObservable;
        private readonly ICachingBinder cachingBinder;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ITaskSourceFactory taskSourceFactory;
        
        private readonly ConcurrentDictionary<Type, object> taskSources = new ConcurrentDictionary<Type, object>();

        public ConfigurationGetter(IConfigurationObservable configurationObservable, ICachingBinder cachingBinder, ISourceDataCache sourceDataCache, ITaskSourceFactory taskSourceFactory)
        {
            this.configurationObservable = configurationObservable;
            this.cachingBinder = cachingBinder;
            this.sourceDataCache = sourceDataCache;
            this.taskSourceFactory = taskSourceFactory;
        }
        
        public TSettings Get<TSettings>()
        {
            var type = typeof(TSettings);
            var taskSource = (ITaskSource<TSettings>) taskSources.GetOrAdd(type, _ => taskSourceFactory.Create<TSettings>());
            return taskSource.Get(configurationObservable.Observe<TSettings>);
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            var cacheItem = sourceDataCache.GetLimitedCacheItem<TSettings>(source);
            try
            {
                var boundSettings = cachingBinder.Bind(source.Get(), cacheItem.BindingCacheItem);
                var lastError = cacheItem.LastValue?.Value.error;
                cacheItem.LastValue = (boundSettings, lastError);
                return boundSettings;
            }
            catch (Exception)
            {
                if (cacheItem.LastValue != null)
                    return cacheItem.LastValue.Value.settings;
                throw;
            }
        }
    }
}