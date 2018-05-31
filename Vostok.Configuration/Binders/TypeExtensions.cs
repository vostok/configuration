using System;

namespace Vostok.Configuration.Binders
{
    internal static class TypeExtensions
    {
        public static bool IsPrimitive(this Type type) =>
            type.IsValueType && type.IsPrimitive;

        public static bool IsNullable(this Type type) =>
            type.IsValueType && type.IsGenericType;
    }
}