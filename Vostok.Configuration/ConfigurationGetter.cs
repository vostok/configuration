using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration
{
    internal class ConfigurationGetter : IConfigurationGetter
    {
        private readonly Func<Type, IConfigurationSource> sourceProvider;
        private readonly IConfigurationObservable configurationObservable;
        private readonly ISourceDataCache sourceDataCache;
        private readonly ITaskSourceFactory taskSourceFactory;

        public ConfigurationGetter(Func<Type, IConfigurationSource> sourceProvider, IConfigurationObservable configurationObservable, ISourceDataCache sourceDataCache, ITaskSourceFactory taskSourceFactory)
        {
            this.sourceProvider = sourceProvider;
            this.configurationObservable = configurationObservable;
            this.sourceDataCache = sourceDataCache;
            this.taskSourceFactory = taskSourceFactory;
        }
        
        public TSettings Get<TSettings>()
        {
            var type = typeof(TSettings);
            var source = sourceProvider(type);
            var cacheItem = sourceDataCache.GetPersistentCacheItem<TSettings>(source);
            if (cacheItem.TaskSource == null)
                cacheItem.TrySetTaskSource(taskSourceFactory.Create(configurationObservable.Observe<TSettings>));
            return cacheItem.TaskSource.Get();
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            var cacheItem = sourceDataCache.GetLimitedCacheItem<TSettings>(source);
            if (cacheItem.TaskSource != null)
                return cacheItem.TaskSource.Get();
            var taskSource = taskSourceFactory.Create(() => configurationObservable.Observe<TSettings>(source));
            var result = taskSource.Get();
            if (!cacheItem.TrySetTaskSource(taskSource) || cacheItem.IsDisposed)
                taskSource.Dispose();
            return result;
        }
    }
}