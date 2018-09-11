using System;
using System.Collections;
using System.Text;
using Vostok.Configuration.Helpers;
using Vostok.Logging.Hercules.Helpers;

namespace Vostok.Configuration.Sources
{
    public static class IniSerializer
    {
        public static string Serialize(object obj)
        {
            var sb = new StringBuilder();
            InnerSerialize(obj, sb);
            return sb.ToString();
        }

        private static void InnerSerialize(object obj, StringBuilder sb, string keyPrefix = null)
        {
            if (obj == null)
            {
                sb.AppendLine($"{keyPrefix ?? "value"} = ");
                return;
            }

            var type = obj.GetType();

            if (TypeIsSimple(type))
                sb.AppendLine($"{keyPrefix ?? "value"} = " + obj);
            else if (IsSimpleDictionary(type))
            {
                foreach (var (key, value) in DictionaryInspector.EnumerateSimpleDictionary(obj))
                    InnerSerialize(value, sb, keyPrefix == null ? key : $"{keyPrefix}.{key}");
            }
            else if (obj is IEnumerable)
                InnerSerialize(null, sb, keyPrefix);
            else if (HasCustomToString(type))
                InnerSerialize(obj.ToString(), sb, keyPrefix);
            else if (HasPublicProperties(type))
            {
                foreach (var (key, value) in ObjectPropertiesExtractor.ExtractProperties(obj))
                    InnerSerialize(value, sb, keyPrefix == null ? key : $"{keyPrefix}.{key}");
            }
            else
                sb.AppendLine($"{keyPrefix ?? "value"} = " + obj);
        }

        private static bool TypeIsSimple(Type type) =>
            type.IsValueType && !type.IsAnsiClass || type == typeof(string) || type == typeof(Enum);

        private static bool HasCustomToString(Type type) =>
            ToStringDetector.HasCustomToString(type);

        private static bool IsSimpleDictionary(Type type) =>
            DictionaryInspector.IsSimpleDictionary(type);

        private static bool HasPublicProperties(Type type) =>
            ObjectPropertiesExtractor.HasProperties(type);
    }
}