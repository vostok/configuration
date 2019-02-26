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
                (key, value) => ((IDisposable)value).Dispose());
            persistentSourceCache = new ConcurrentDictionary<(IConfigurationSource, Type), object>();
        }

        public SourceCacheItem<TSettings> GetLimitedCacheItem<TSettings>(IConfigurationSource source)
        {
            var key = (source, typeof(TSettings));

            if (limitedSourceCache.TryGetValue(key, out var value))
                return (SourceCacheItem<TSettings>)value;

            return (SourceCacheItem<TSettings>)limitedSourceCache.GetOrAdd(key, _ => new SourceCacheItem<TSettings>());
        }

        public SourceCacheItem<TSettings> GetPersistentCacheItem<TSettings>(IConfigurationSource source)
        {
            var key = (source, typeof(TSettings));
            return (SourceCacheItem<TSettings>)persistentSourceCache.GetOrAdd(key, _ => new SourceCacheItem<TSettings>());
        }

        public void Dispose()
        {
            foreach (var cacheItem in persistentSourceCache.Values.Concat(limitedSourceCache.Values).Cast<IDisposable>())
                cacheItem.Dispose();
        }
    }
}
