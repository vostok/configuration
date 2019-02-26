using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Helpers
{
    internal static class SettingsNodeExtensions
    {
        public static bool IsMissing(this ISettingsNode node) => node == null;

        public static bool IsNullValue(this ISettingsNode node) =>
            node is ValueNode valueNode && valueNode.Value == null;

        public static bool IsNullValue<T>(this ISettingsNode node, ISafeSettingsBinder<T> binder) =>
            binder is INullValuePolicy policy ? policy.IsNullValue(node) : node.IsNullValue();

        public static bool IsNullOrMissing<T>(this ISettingsNode node, ISafeSettingsBinder<T> binder) =>
            node.IsMissing() || node.IsNullValue(binder);
    }
}