using System;
using SimpleInjector;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBinderFactory : ISettingsBinderFactory
    {
        private readonly Container container;

        public SettingsBinderFactory(Container container) =>
            this.container = container;

        public ISettingsBinder<T> CreateFor<T>() =>
            container.GetInstance<ISettingsBinder<T>>();

        public ISettingsBinder<object> CreateForType(Type type, BinderAttribute binderAttribute = BinderAttribute.IsRequired) =>
            new BinderWrapper(container.GetInstance(typeof(ISettingsBinder<>).MakeGenericType(type)), binderAttribute);
    }
}