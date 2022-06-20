using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Cache
{
    internal class SourceDataCache : ISourceDataCache
    {
        private readonly WindowedCache<(IConfigurationSource, Type), SourceCacheItem> limitedSourceCache;
        private readonly ConcurrentDictionary<(IConfigurationSource, Type), SourceCacheItem> persistentSourceCache;

        public SourceDataCache(int limitedCacheCapacity)
        {
            limitedSourceCache = new WindowedCache<(IConfigurationSource, Type), SourceCacheItem>(
                limitedCacheCapacity,
                (key, value) => ((IDisposable)value).Dispose());
            persistentSourceCache = new ConcurrentDictionary<(IConfigurationSource, Type), SourceCacheItem>();
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

        public IEnumerable<SourceCacheItem> GetAll() =>
            persistentSourceCache.Values.Concat(limitedSourceCache.Values);

        public void Dispose()
        {
            foreach (var cacheItem in persistentSourceCache.Values.Concat(limitedSourceCache.Values).Cast<IDisposable>())
                cacheItem.Dispose();
        }
    }
}