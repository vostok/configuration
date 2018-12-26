using System;
using System.Collections.Concurrent;
using System.Linq;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Cache
{
    internal class SourceDataCache : ISourceDataCache
    {
        private readonly WindowedCache<(IConfigurationSource, Type), object> limitedSourceCache;
        private readonly ConcurrentDictionary<(IConfigurationSource, Type), object> persistentSourceCache;

        public SourceDataCache(int limitedCacheCapacity)
        {
            limitedSourceCache = new WindowedCache<(IConfigurationSource, Type), object>(
                limitedCacheCapacity,
                kv => ((IDisposable)kv.Value).Dispose());
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
            return (SourceCacheItem<TSettings>)GetFromLimitedOrPersistentCache(key, newItem, limitedSourceCache, persistentSourceCache);
        }

        public void Dispose()
        {
            foreach (var cacheItem in persistentSourceCache.Values.Concat(limitedSourceCache.Values).Cast<IDisposable>())
                cacheItem.Dispose();
        }

        private static object GetFromLimitedOrPersistentCache<T>(T key, object newItem, WindowedCache<T, object> limitedCache, ConcurrentDictionary<T, object> persistentCache)
        {
            if (limitedCache.TryRemove(key, out var result))
                persistentCache.AddOrUpdate(key, result, (k, obj) => result);
            else
                result = persistentCache.GetOrAdd(key, newItem);

            return result;
        }
    }
}