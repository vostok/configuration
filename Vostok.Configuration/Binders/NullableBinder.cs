using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class NullableBinder<T> : ISafeSettingsBinder<T?>, INullValuePolicy
        where T : struct
    {
        private readonly ISafeSettingsBinder<T> valueBinder;

        public NullableBinder(ISafeSettingsBinder<T> valueBinder) =>
            this.valueBinder = valueBinder;

        public SettingsBindingResult<T?> Bind(ISettingsNode settings)
        {
            if (settings.IsNullOrMissing(this))
                return SettingsBindingResult.Success(null as T?);

            return valueBinder.Bind(settings).ConvertToNullable();
        }

        public bool IsNullValue(ISettingsNode node)
        {
            if (node.IsNullValue())
                return true;

            return node is ValueNode valueNode && valueNode.Value?.ToLower() == "null";
        }
    }
}