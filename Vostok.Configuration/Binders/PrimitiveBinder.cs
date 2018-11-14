﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveBinder<T> : ISettingsBinder<T>
    {
        private readonly IDictionary<Type, ITypeParser> parsers;

        public PrimitiveBinder(IDictionary<Type, ITypeParser> parsers) =>
            this.parsers = parsers;

        public T Bind(ISettingsNode settings)
        {
            var valueNode = settings as ValueNode;
            if (valueNode == null && settings.Children.Count() == 1)
                valueNode = settings.Children.Single() as ValueNode;

            if (valueNode == null)
                throw new SettingsBindingException($"Provided settings node of type '{settings.GetType()}' cannot be bound by {nameof(PrimitiveBinder<T>)}.");

            if (valueNode.Value == null && !typeof(T).IsValueType)
                return default;

            if (!parsers.TryGetValue(typeof(T), out var parser))
                throw new SettingsBindingException($"There is no parser configured for primitive type '{typeof(T)}'.");

            if (!parser.TryParse(valueNode.Value, out var result))
                throw new SettingsBindingException($"Value '{valueNode.Value}' cannot be parsed as '{typeof(T)}'.");

            return (T)result;
        }
    }
}