using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vostok.Configuration
{
    internal class ChildrenKeysComparer: IComparer<string>
    {
        /*public new bool Equals(object x, object y)
        {
            if (x is string sx && y is string sy)
                return string.Equals(sx.ToLower(CultureInfo.CurrentCulture), sy.ToLower(CultureInfo.CurrentCulture));
            return object.Equals(x, y);
        }

        public int GetHashCode(object obj)
        {
            if (obj is string s)
                return s.ToLower(CultureInfo.CurrentCulture).GetHashCode();
            return obj.GetHashCode();
        }*/

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            return string.Compare(
                x.ToLower(CultureInfo.InvariantCulture),
                y.ToLower(CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
        }
    }
}