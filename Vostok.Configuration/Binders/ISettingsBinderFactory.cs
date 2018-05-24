using System;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal interface ISettingsBinderFactory
    {
        ISettingsBinder<T> CreateFor<T>();
        ISettingsBinder<object> CreateForType(Type type, BinderAttribute binderAttribute = BinderAttribute.IsRequired);
    }
}