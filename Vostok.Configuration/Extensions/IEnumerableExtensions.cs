using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Extensions
{
    public static class IEnumerableExtensions
    {
        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null) =>
            new SortedDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector), comparer);
    }
}