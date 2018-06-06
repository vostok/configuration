using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Vostok.Configuration.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IOrderedDictionary ToOrderedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
                                                                                      Func<TSource, TKey> keySelector,
                                                                                      Func<TSource, TElement> elementSelector,
                                                                                      IEqualityComparer comparer = null)
        {
            var pairs = source as TSource[] ?? source.ToArray();
            var result = new OrderedDictionary(pairs.Length, comparer);
            foreach (var data in pairs)
                result.Add(keySelector(data), elementSelector(data));
            return result;
        }
    }
}