using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Extensions
{
    internal static class SettingsBinderExtensions
    {
        public static SettingsBindingResult<TSettings> BindOrDefault<TSettings>(this ISafeSettingsBinder<TSettings> binder, ISettingsNode node) =>
            node.IsNullValue(binder) ? SettingsBindingResult.Success<TSettings>(default) : binder.Bind(node);
    }
}