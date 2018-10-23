using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper<T> : ISettingsBinder<object>
    {
        private readonly ISettingsBinder<T> binder;

        public BinderWrapper(ISettingsBinder<T> binder) =>
            this.binder = binder;

        public object Bind(ISettingsNode rawSettings) => binder.Bind(rawSettings);
    }
}