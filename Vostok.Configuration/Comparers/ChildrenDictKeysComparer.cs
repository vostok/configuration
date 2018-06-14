using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vostok.Configuration.Comparers
{
    internal class ChildrenKeysEqualityComparer: IEqualityComparer<string>
    {
        public bool Equals(string x, string y) =>
            string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);

        public int GetHashCode(string str) =>
            str.ToLower(CultureInfo.InvariantCulture).GetHashCode();
    }
}