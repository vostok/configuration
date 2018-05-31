using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Vostok.Configuration.Extensions
{
    // CR(krait): Dead code.
    public static class IOrderedDictionaryExtensions
    {
        /*public static IDictionary<TKey, TElement> ToDictionary<TKey, TElement>(this IOrderedDictionary source, Func<DictionaryEntry, TKey> keySelector, Func<DictionaryEntry, TElement> elementSelector)
        {
            var result = new Dictionary<TKey, TElement>();
            foreach (DictionaryEntry pair in source)
                result.Add(keySelector(pair), elementSelector(pair));

            return result;
        }*/

        public static IDictionary<TKey, TElement> CastToDictionary<TKey, TElement>(this IOrderedDictionary source)
        {
            var result = new Dictionary<TKey, TElement>();
            foreach (DictionaryEntry pair in source)
                if (pair.Key is TKey key)
                    result.Add(key, (TElement)pair.Value);

            return result;
        }
    }
}