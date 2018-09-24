using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveAndSimpleBinder<T> : ISettingsBinder<T>
    {
        private readonly IDictionary<Type, ITypeParser> parsers;

        public PrimitiveAndSimpleBinder(IDictionary<Type, ITypeParser> parsers) =>
            this.parsers = parsers;

        public T Bind(ISettingsNode settings)
        {
            var type = typeof(T);
            var typeIsString = type == typeof(string);
            if (!parsers.ContainsKey(type) && !typeIsString)
                throw new ArgumentException($"{nameof(PrimitiveAndSimpleBinder<T>)}: have no parser for the type \"{type.Name}\"");

            string value;
            if (type.IsValueType)
            {
                SettingsNode.CheckSettings(settings);
                if (!string.IsNullOrWhiteSpace(settings.Value))
                    value = settings.Value;
                else if (settings.Children.Count() == 1 && settings.Children.First() is ValueNode valueNode && !string.IsNullOrWhiteSpace(valueNode.Value))
                    value = valueNode.Value;
                else
                    throw new ArgumentNullException($"{nameof(PrimitiveAndSimpleBinder<T>)}: settings value is null. Can't parse.");
            }
            else if (!type.IsValueType && !typeIsString)
            {
                SettingsNode.CheckSettings(settings, false);
                if (settings.Value != null)
                    value = settings.Value;
                else if (settings.Children.Count() == 1 && settings.Children.First() is ValueNode valueNode && valueNode.Value != null)
                    value = valueNode.Value;
                else
                    return (T) type.Default();
            }
            else
            {
                if (settings.Children.Count() == 1)
                    return (T) (object) settings.Children.First().Value;
                return (T) (object) settings.Value;
            }

            if (parsers[type].TryParse(value, out var res))
                return (T) res;

            throw new InvalidCastException($"{nameof(PrimitiveAndSimpleBinder<T>)}: can't parse into specified type \"{type.Name}\"");
        }
    }
}