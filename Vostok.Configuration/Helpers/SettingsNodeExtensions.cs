using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Helpers
{
    internal static class SettingsNodeExtensions
    {
        public static bool IsNullValue(this ISettingsNode node) => 
            node != null && node is ValueNode valueNode && valueNode.Value == null;

        public static bool IsNullValue<T>(this ISettingsNode node, ISettingsBinder<T> binder) => 
            binder is INullValuePolicy policy ? policy.IsNullValue(node) : node.IsNullValue();
    }
}