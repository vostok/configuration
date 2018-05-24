using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Vostok.Configuration.Sources
{
    public static class IEnumerableExtensions
    {
        public static IOrderedDictionary ToOrderedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var result = new OrderedDictionary();
            foreach (var data in source)
                result.Add(keySelector(data), elementSelector(data));
            return result;
        }
    }
}