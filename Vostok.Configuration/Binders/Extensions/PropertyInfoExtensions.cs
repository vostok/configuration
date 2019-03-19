using System.Reflection;

namespace Vostok.Configuration.Binders.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static void ForceSetValue(this PropertyInfo property, object obj, object value)
        {
            if (property.CanWrite)
            {
                property.SetValue(obj, value);
                return;
            }

            var backingField = property.DeclaringType?.GetField($"<{property.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (backingField != null)
                backingField.SetValue(obj, value);
        }

        public static bool IsAbstract(this PropertyInfo propertyInfo) =>
            (propertyInfo.GetMethod?.IsAbstract ?? false) || (propertyInfo.SetMethod?.IsAbstract ?? false);

        public static bool IsVirtual(this PropertyInfo propertyInfo) =>
            (propertyInfo.GetMethod?.IsVirtual ?? false) || (propertyInfo.SetMethod?.IsVirtual ?? false);
    }
}