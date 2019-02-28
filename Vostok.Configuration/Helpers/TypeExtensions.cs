using System;
using System.Linq;
using System.Reflection;

namespace Vostok.Configuration.Helpers
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        public static PropertyInfo[] GetInstanceProperties(this Type type) =>
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => !pi.PropertyType.IsAbstract)
                .ToArray();

        public static FieldInfo[] GetInstanceFields(this Type type) =>
            type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => !pi.FieldType.IsAbstract)
                .ToArray();
    }
}