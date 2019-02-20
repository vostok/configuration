using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly Lazy<Container> containerWrapper;
        private readonly ConcurrentQueue<Action<Container>> customBinderRegistrations = 
            new ConcurrentQueue<Action<Container>>();

        public SettingsBinderProvider()
        {
            containerWrapper = new Lazy<Container>(
                () =>
                {
                    var container = new Container();

                    foreach (var registration in customBinderRegistrations)
                        registration(container);
                    
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

                    container.Verify();

                    return container;
                }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ISettingsBinder<T> CreateFor<T>()
        {
            if (TryObtainBindByBinder<T>(typeof(T), false, out var binder))
                return binder;
            
            return containerWrapper.Value.GetInstance<ISettingsBinder<T>>();
        }

        public ISettingsBinder<object> CreateFor(Type type)
        {
            if (TryObtainBindByBinder<object>(type, true, out var binder))
                return binder;
            
            return (ISettingsBinder<object>)containerWrapper.Value.GetInstance(typeof(BinderWrapper<>).MakeGenericType(type));
        }
        
        public void SetupCustomBinder<TValue>(ISettingsBinder<TValue> binder)
        {
            EnsureCanRegisterBinders();
            
            customBinderRegistrations.Enqueue(container => container.RegisterInstance(typeof(ISettingsBinder<TValue>), binder));
        }
        
        public void SetupCustomBinder(Type binderType, Predicate<Type> condition)
        {
            EnsureCanRegisterBinders();

            customBinderRegistrations.Enqueue(container => 
                container.RegisterConditional(typeof(ISettingsBinder<>), binderType, c => condition(c.ServiceType.GetGenericArguments()[0])));
        }

        private void EnsureCanRegisterBinders()
        {
            if (containerWrapper.IsValueCreated)
                throw new InvalidOperationException($"Cannot register custom binders after the container was used.");
        }

        public void SetupParserFor<T>(ITypeParser parser)
        {
            SetupCustomBinder(new PrimitiveBinder<T>(parser));
        }
        
        private static bool TryObtainBindByBinder<T>(Type type, bool wrap, out ISettingsBinder<T> settingsBinder)
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