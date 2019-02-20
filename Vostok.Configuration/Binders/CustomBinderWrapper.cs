using System;
using System.Collections.Generic;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class CustomBinderWrapper<T> : ISettingsBinder<T>, INullValuePolicy
    {
        internal ISettingsBinder<T> Binder { get; }

        public CustomBinderWrapper(IDictionary<Type, object> binders) => Binder = (ISettingsBinder<T>)binders[typeof(T)];

        public SettingsBindingResult<T> Bind(ISettingsNode settings) => Binder.Bind(settings);
        
        public bool IsNullValue(ISettingsNode node) => node.IsNullValue(Binder);
    }
}