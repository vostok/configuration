using System.Collections.Generic;

namespace Vostok.Configuration
{
    // TODO(krait): Move to commons.
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}