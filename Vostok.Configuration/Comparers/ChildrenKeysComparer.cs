using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vostok.Configuration.Comparers
{
    // CR(krait): Why couldn't you just use StringComparer.InvariantCultureIgnoreCase?
    internal class ChildrenKeysComparer : IComparer<string>
    {
        public int Compare(string x, string y) =>
            string.Compare(
                x?.ToLower(CultureInfo.InvariantCulture),
                y?.ToLower(CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
    }
}