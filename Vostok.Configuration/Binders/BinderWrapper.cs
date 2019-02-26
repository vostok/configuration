using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper<T> : ISafeSettingsBinder<object>, INullValuePolicy
    {
        private readonly ISafeSettingsBinder<T> binder;

        public BinderWrapper(ISafeSettingsBinder<T> binder) => this.binder = binder;

        public SettingsBindingResult<object> Bind(ISettingsNode rawSettings) =>
            binder.Bind(rawSettings).Convert<T, object>();

        public bool IsNullValue(ISettingsNode node) => node.IsNullValue(binder);
    }
}