﻿using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Helpers
{
    internal static class SettingsBinderExtensions
    {
        public static SettingsBindingResult<TSettings> BindOrDefault<TSettings>(this ISafeSettingsBinder<TSettings> binder, ISettingsNode node) =>
            node.IsNullValue(binder) ? SettingsBindingResult.Success<TSettings>(default) : binder.Bind(node);
    }
}