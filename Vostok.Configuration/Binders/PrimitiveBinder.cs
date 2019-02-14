using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveBinder<T> : ISettingsBinder<T>
    {
        private readonly ITypeParser parser;

        public PrimitiveBinder(ITypeParser parser) =>
            this.parser = parser;

        public T Bind(ISettingsNode settings)
        {
            if (settings == null)
                return default;
            
            var valueNode = settings as ValueNode;
            if (valueNode == null && settings.Children.Count() == 1)
                valueNode = settings.Children.Single() as ValueNode;

            if (valueNode == null)
                throw new SettingsBindingException($"Provided settings node of type '{settings.GetType()}' cannot be bound by {nameof(PrimitiveBinder<T>)}.");

            if (valueNode.Value == null && !typeof(T).IsValueType)
                return default;

            if (!parser.TryParse(valueNode.Value, out var result))
                throw new SettingsBindingException($"Value '{valueNode.Value}' cannot be parsed as '{typeof(T)}'.");

            return (T)result;
        }
    }
}