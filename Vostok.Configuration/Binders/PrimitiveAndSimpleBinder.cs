﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveAndSimpleBinder<T> : ISettingsBinder<T>
    {
        private readonly IDictionary<Type, ITypeParser> parsers;

        public PrimitiveAndSimpleBinder(IDictionary<Type, ITypeParser> parsers) =>
            this.parsers = parsers;

        public T Bind(IRawSettings settings)
        {
            var type = typeof(T);
            if (!parsers.ContainsKey(type) && type != typeof(string))
                throw new ArgumentException($"{nameof(PrimitiveAndSimpleBinder<T>)}: have no parser for the type \"{type.Name}\"");
            RawSettings.CheckSettings(settings);

            string value;
            if (!string.IsNullOrWhiteSpace(settings.Value))
                value = settings.Value;
            else if (settings.Value == null && settings.Children.Count() == 1)
                value = settings.Children.First().Value;
            else
                throw new ArgumentNullException($"{nameof(PrimitiveAndSimpleBinder<T>)}: settings value is null. Can't parse.");

            if (type == typeof(string))
                return (T)(object)value;
            if (parsers[type].TryParse(value, out var res))
                return (T)res;

            throw new InvalidCastException($"{nameof(PrimitiveAndSimpleBinder<T>)}: can't parse into specified type \"{type.Name}\"");
        }
    }
}