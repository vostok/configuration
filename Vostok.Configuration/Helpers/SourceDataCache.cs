using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Helpers
{
    internal class SourceDataCache : ISourceDataCache
    {
        private readonly WindowedCache<(IConfigurationSource, Type), object> limitedSourceCache;
        private readonly ConcurrentDictionary<(IConfigurationSource, Type), object> persistentSourceCache;

        public SourceDataCache(int limitedCacheCapacity)
        {
            limitedSourceCache = new WindowedCache<(IConfigurationSource, Type), object>(limitedCacheCapacity);
            persistentSourceCache = new ConcurrentDictionary<(IConfigurationSource, Type), object>();
        }

        public SourceCacheItem<TSettings> GetLimitedCacheItem<TSettings>(IConfigurationSource source)
        {
            var key = (source, typeof(TSettings));
            var newItem = new SourceCacheItem<TSettings>();
            return (SourceCacheItem<TSettings>)limitedSourceCache.GetOrAdd(key, _ => newItem);
        }
        
        public SourceCacheItem<TSettings> GetPersistentCacheItem<TSettings>(IConfigurationSource source)
        {
            var key = (source, typeof(TSettings));
            var newItem = new SourceCacheItem<TSettings>();
            if (limitedSourceCache.TryRemove(key, out var result))
                persistentSourceCache.AddOrUpdate(key, result, (k, obj) => result);
            else
                result = persistentSourceCache.GetOrAdd(key, newItem);
            return (SourceCacheItem<TSettings>)result;
        }
    }
}