using System;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper<T> : ISafeSettingsBinder<object>, INullValuePolicy, IBinderWrapper
    {
        private readonly ISafeSettingsBinder<T> binder;

        public BinderWrapper(ISafeSettingsBinder<T> binder) => this.binder = binder;

        public Type BinderType => binder.GetType();

        public SettingsBindingResult<object> Bind(ISettingsNode rawSettings) =>
            binder.Bind(rawSettings).Convert<T, object>();

        public bool IsNullValue(ISettingsNode node) => node.IsNullValue(binder);
    }
}