using System;

namespace Vostok.Configuration.Binders
{
    internal interface ISettingsBinderFactory
    {
        ISettingsBinder<T> CreateFor<T>();
        ISettingsBinder<object> CreateForType(Type type);
    }
}