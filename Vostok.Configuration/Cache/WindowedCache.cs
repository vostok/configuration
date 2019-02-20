using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Cache
{
    internal class WindowedCache<TKey, TValue>
        where TValue : class
    {
        private readonly int capacity;
        private readonly Action<TKey, TValue> onAutoRemove;
        private readonly ConcurrentDictionary<TKey, TValue> cache = new ConcurrentDictionary<TKey, TValue>();
        private readonly ConcurrentQueue<TKey> queue = new ConcurrentQueue<TKey>();

        public WindowedCache(int capacity, Action<TKey, TValue> onAutoRemove)
        {
            this.capacity = capacity;
            this.onAutoRemove = onAutoRemove;
        }

        public IEnumerable<TValue> Values => cache.Select(pair => pair.Value);

        public bool TryGetValue(TKey key, out TValue value) => cache.TryGetValue(key, out value);

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue newValue = null;
            var value = cache.GetOrAdd(key, _ => newValue = valueFactory(key));
            if (ReferenceEquals(value, newValue))
                queue.Enqueue(key);

            RemoveOutOfWindowItems();

            return value;
        }

        public bool TryRemove(TKey key, out TValue value) => cache.TryRemove(key, out value);

        private void RemoveOutOfWindowItems()
        {
            while (queue.Count > capacity && queue.TryDequeue(out var keyToRemove))
                if (cache.TryRemove(keyToRemove, out var removedValue))
                    onAutoRemove(keyToRemove, removedValue);
        }
    }
}