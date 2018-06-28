using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null) =>
            new SortedDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector), comparer);

        public static IEnumerable<T> Unique<T>(this IEnumerable<T> source, IEnumerable<T> list, IEqualityComparer<T> comparer = null)
        {
            if (source == null) return list;
            if (list == null) return source;

            var src = source as T[] ?? source.ToArray();
            var lst = list as T[] ?? list.ToArray();

            var unique1 = src.Except(lst, comparer);
            var unique2 = lst.Except(src, comparer);
            return unique1.Concat(unique2).ToArray();
        }
    }
}