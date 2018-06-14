using System;
using SimpleInjector;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBinderFactory : ISettingsBinderFactory
    {
        private readonly Container container;

        public SettingsBinderFactory(Container container) =>
            this.container = container;

        public ISettingsBinder<T> CreateFor<T>() =>
            container.GetInstance<ISettingsBinder<T>>();

        public ISettingsBinder<object> CreateFor(Type type) =>
            new BinderWrapper(container.GetInstance(typeof(ISettingsBinder<>).MakeGenericType(type)));
    }
}