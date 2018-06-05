using System.Collections;
using System.Globalization;

namespace Vostok.Configuration
{
    internal class ChildrenKeysComparer: IEqualityComparer
    {
        public new bool Equals(object x, object y)
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
        }
    }
}