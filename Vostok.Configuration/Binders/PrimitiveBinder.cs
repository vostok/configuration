using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveBinder<T> : ISettingsBinder<T>, INullValuePolicy
    {
        private readonly ITypeParser parser;

        public PrimitiveBinder(ITypeParser parser) =>
            this.parser = parser;

        public SettingsBindingResult<T> Bind(ISettingsNode settings)
        {
            var valueNode = settings as ValueNode;
            if (valueNode == null && settings.Children.Count() == 1)
                valueNode = settings.Children.Single() as ValueNode;

            if (valueNode == null)
                return SettingsBindingResult.NodeTypeMismatch<T>(settings);

            if (!parser.TryParse(valueNode.Value, out var result))
                return SettingsBindingResult.ParsingError<T>(valueNode.Value);

            return SettingsBindingResult.Success((T)result);
        }

        public bool IsNullValue(ISettingsNode node)
        {
            if (node.IsNullValue())
                return true;

            if (node?.Children.Count() != 1)
                return false;

            return node.Children.Single().IsNullValue();
        }
    }
}