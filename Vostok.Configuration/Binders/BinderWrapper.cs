using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper<T> : ISettingsBinder<object>, INullValuePolicy
    {
        private readonly ISettingsBinder<T> binder;

        public BinderWrapper(ISettingsBinder<T> binder) => this.binder = binder;

        public object Bind(ISettingsNode rawSettings) => binder.Bind(rawSettings);

        public bool IsNullValue(ISettingsNode node) => node.IsNullValue(binder);
    }
}