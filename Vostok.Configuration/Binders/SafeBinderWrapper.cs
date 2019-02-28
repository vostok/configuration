using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class SafeBinderWrapper<T> : ISafeSettingsBinder<T>
    {
        public SafeBinderWrapper(ISettingsBinder<T> binder) => Binder = binder;
        public ISettingsBinder<T> Binder { get; }

        public SettingsBindingResult<T> Bind(ISettingsNode rawSettings) =>
            SettingsBindingResult.Catch(() => SettingsBindingResult.Success(Binder.Bind(rawSettings)));
    }
}