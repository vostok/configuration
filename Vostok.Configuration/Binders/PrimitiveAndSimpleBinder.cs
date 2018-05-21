using System;
using System.Linq;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveAndSimpleBinder<T> : ISettingsBinder<T>
    {
        public static bool IsAvailableType(Type type) =>
            type.IsPrimitive() ||
            type == typeof(string) ||
            PrimitiveAndSimpleParsers.Parsers.ContainsKey(type);

        public T Bind(RawSettings settings)
        {
            var type = typeof(T);
            if (!PrimitiveAndSimpleParsers.Parsers.ContainsKey(type) && type != typeof(string))
                throw new ArgumentException("Wrong type");
            RawSettings.CheckSettings(settings);

            string value;
            if (!string.IsNullOrWhiteSpace(settings.Value))
                value = settings.Value;
            else if (settings.Value == null && settings.Children == null && settings.ChildrenByKey != null && settings.ChildrenByKey.Count == 1)
                value = settings.ChildrenByKey.First().Value.Value;
            else
                throw new ArgumentNullException("Value is null");

            if (type == typeof(string))
                return (T)(object)value;
            if (PrimitiveAndSimpleParsers.Parsers[type].TryParse(value, out var res))
                return (T)res;

            throw new InvalidCastException("Wrong type");
        }
    }
}