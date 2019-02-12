using System;

namespace Vostok.Configuration.Helpers
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;
    }
}