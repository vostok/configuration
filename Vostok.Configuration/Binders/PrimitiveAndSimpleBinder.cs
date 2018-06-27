using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.SettingsTree;

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
            if (!typeIsString)
            {
                SettingsNode.CheckSettings(settings);
                if (!string.IsNullOrWhiteSpace(settings.Value))
                    value = settings.Value;
                else if (settings.Value == null && settings.Children.Count() == 1 && settings.Children.First() is ValueNode valueNode)
                    value = valueNode.Value;
                else
                    throw new ArgumentNullException($"{nameof(PrimitiveAndSimpleBinder<T>)}: settings value is null. Can't parse.");
            }
            else
                return (T)(object)settings.Value;

            if (parsers[type].TryParse(value, out var res))
                return (T)res;

            throw new InvalidCastException($"{nameof(PrimitiveAndSimpleBinder<T>)}: can't parse into specified type \"{type.Name}\"");
        }
    }
}