using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Extensions
{
    internal static class SettingsNodeExtensions
    {
        public static bool IsNull(this ISettingsNode node) => node is ValueNode valueNode && valueNode.Value == null;
    }
}