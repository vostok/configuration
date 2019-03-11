using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class UnsafeBinderWrapper<T> : ISettingsBinder<T>
    {
        public UnsafeBinderWrapper(ISafeSettingsBinder<T> binder) => Binder = binder;
        public ISafeSettingsBinder<T> Binder { get; }

        public T Bind(ISettingsNode rawSettings) => Binder.Bind(rawSettings).Value;
    }
}