using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Helpers;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBinderProvider : ISettingsBinderProvider
    {
        private readonly Container container;
        private readonly Dictionary<Type, object> customBinders;
        private readonly ConcurrentDictionary<Type, bool> setupDisabled = new ConcurrentDictionary<Type, bool>();

        public SettingsBinderProvider()
        {
            customBinders = new Dictionary<Type, object>();

            container = new Container();
            container.RegisterConditional(
                typeof(ISettingsBinder<>),
                typeof(CustomBinderWrapper<>),
                c => customBinders.ContainsKey(c.ServiceType.GetGenericArguments()[0]));
            container.RegisterConditional(
                typeof(ISettingsBinder<>),
                typeof(NullableBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsNullable());
            container.RegisterConditional(
                typeof(ISettingsBinder<>),
                typeof(EnumBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsEnum);
            container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ReadOnlyListBinder<>), c => !c.Handled);
            container.Register(typeof(ISettingsBinder<>), typeof(DictionaryBinder<,>));
            container.Register(typeof(ISettingsBinder<>), typeof(SetBinder<>));
            container.RegisterConditional(
                typeof(ISettingsBinder<>),
                typeof(ClassStructBinder<>),
                c => !c.Handled);
            container.RegisterInstance(typeof(ISettingsBinderProvider), this);
            container.RegisterInstance(typeof(IDictionary<Type, object>), customBinders);

            container.Verify();
        }

        public ISettingsBinder<T> CreateFor<T>()
        {
            if (TryObtainBindByBinder<T>(typeof(T), false, out var binder))
                return binder;
            
            setupDisabled[typeof(T)] = true;
            return container.GetInstance<ISettingsBinder<T>>();
        }

        public ISettingsBinder<object> CreateFor(Type type)
        {
            if (TryObtainBindByBinder<object>(type, true, out var binder))
                return binder;
            
            setupDisabled[type] = true;
            return (ISettingsBinder<object>)container.GetInstance(typeof(BinderWrapper<>).MakeGenericType(type));
        }
        
        public void SetupCustomBinder<T>(ISettingsBinder<T> binder)
        {
            var type = typeof(T);
            EnsureSetupEnabledFor(type);
            customBinders[type] = binder;
        }

        public void SetupParserFor<T>(ITypeParser parser)
        {
            SetupCustomBinder(new PrimitiveBinder<T>(parser));
        }

        private void EnsureSetupEnabledFor(Type type)
        {
            if (setupDisabled.ContainsKey(type))
                throw new InvalidOperationException($"Cannot set up custom binder for type '{type}' after {nameof(CreateFor)}() was called for this type.");
        }

        private bool TryObtainBindByBinder<T>(Type type, bool wrap, out ISettingsBinder<T> settingsBinder)
        {
            settingsBinder = null;
            
            if (!(type.GetCustomAttributes(typeof(BindByAttribute), false).FirstOrDefault() is BindByAttribute bindByAttribute))
                return false;

            if (!typeof(ISettingsBinder<>).MakeGenericType(type).IsAssignableFrom(bindByAttribute.BinderType))
                return false;

            var binder = Activator.CreateInstance(bindByAttribute.BinderType);
            if (wrap)
                binder = Activator.CreateInstance(typeof(BinderWrapper<>).MakeGenericType(type), binder);

            settingsBinder = (ISettingsBinder<T>)binder;
            return true;
        }
    }
}