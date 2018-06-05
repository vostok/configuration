using System;

namespace Vostok.Configuration.Extensions
{
    internal static class TypeExtention
    {
        public static bool IsPrimitive(this Type type) =>
            type.IsValueType && type.IsPrimitive;

        public static bool IsNullable(this Type type) =>
            type.IsValueType && type.IsGenericType;

        public static object Default(this Type type) =>
            !type.IsValueType || type.IsNullable() ? null : Activator.CreateInstance(type);
    }
}