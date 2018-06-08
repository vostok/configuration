using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vostok.Configuration
{
    internal class ChildrenDictKeysComparer: IEqualityComparer<string>
    {
        public bool Equals(string x, string y) =>
            string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);

        public int GetHashCode(string str) =>
            str.ToLower(CultureInfo.CurrentCulture).GetHashCode();
    }
}