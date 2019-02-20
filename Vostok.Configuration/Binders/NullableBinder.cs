using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class NullableBinder<T> : ISettingsBinder<T?>, INullValuePolicy
        where T : struct
    {
        private readonly ISettingsBinder<T> valueBinder;

        public NullableBinder(ISettingsBinder<T> valueBinder) =>
            this.valueBinder = valueBinder;

        public T? Bind(ISettingsNode settings) => valueBinder.Bind(settings);

        public bool IsNullValue(ISettingsNode node)
        {
            if (node.IsNullValue())
                return true;

            return node is ValueNode valueNode && valueNode.Value?.ToLower() == "null";
        }
    }
}