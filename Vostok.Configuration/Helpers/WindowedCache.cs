using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Vostok.Configuration.Helpers
{
    internal class WindowedCache<TKey, TValue>
    {
        private readonly int capacity;
        private readonly ConcurrentDictionary<TKey, TValue> cache = new ConcurrentDictionary<TKey, TValue>();
        private readonly ConcurrentQueue<TKey> queue = new ConcurrentQueue<TKey>();

        public WindowedCache(int capacity)
        {
            this.capacity = capacity;
        }
        
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!cache.ContainsKey(key))
                queue.Enqueue(key);
            var value = cache.GetOrAdd(key, _ => valueFactory(key));
            RemoveOutOfWindowItems();
            return value;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            return cache.TryRemove(key, out value);
        }
        
        private void RemoveOutOfWindowItems()
        {
            while (queue.Count > capacity && queue.TryDequeue(out var keyToRemove))
                cache.TryRemove(keyToRemove, out _);
        }
    }
}