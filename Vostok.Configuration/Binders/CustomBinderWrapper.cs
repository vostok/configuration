using System;
using System.Collections.Generic;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class CustomBinderWrapper<T>: ISettingsBinder<T>
    {
        internal ISettingsBinder<T> Binder { get; }

        public CustomBinderWrapper(IDictionary<Type, object> binders) => Binder = (ISettingsBinder<T>)binders[typeof(T)];

        public T Bind(ISettingsNode settings)
        {
            return Binder.Bind(settings);
        }
    }
}