using System;
using System.Collections.Concurrent;
using System.Threading;
using SimpleInjector;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Extensions;
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
                        typeof(ISafeSettingsBinder<>),
                        typeof(NullableBinder<>),
                        c => c.ServiceType.GetGenericArguments()[0].IsNullable());
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(EnumBinder<>),
                        c => c.ServiceType.GetGenericArguments()[0].IsEnum);
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(ListBinder<>));
                    container.RegisterConditional(typeof(ISafeSettingsBinder<>), typeof(ReadOnlyListBinder<>), c => !c.Handled);
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(DictionaryBinder<,>));
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(SetBinder<>));
                    container.Register(typeof(ISafeSettingsBinder<ISettingsNode>), typeof(IdentityBinder));
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(ClassStructBinder<>),
                        c => !c.Handled);
                    container.RegisterInstance(typeof(ISettingsBinderProvider), this);

                    container.Verify();

                    return container;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ISafeSettingsBinder<T> CreateFor<T>()
        {
            if (typeof(T).TryObtainBindByBinder<T>(false, out var binder))
                return binder;

            return containerWrapper.Value.GetInstance<ISafeSettingsBinder<T>>();
        }

        public ISafeSettingsBinder<object> CreateFor(Type type)
        {
            if (type.TryObtainBindByBinder<object>(true, out var binder))
                return binder;

            return (ISafeSettingsBinder<object>)containerWrapper.Value.GetInstance(typeof(BinderWrapper<>).MakeGenericType(type));
        }

        public void SetupCustomBinder<TValue>(ISafeSettingsBinder<TValue> binder)
        {
            EnsureCanRegisterBinders();

            customBinderRegistrations.Enqueue(container => container.RegisterInstance(typeof(ISafeSettingsBinder<TValue>), binder));
        }

        public void SetupCustomBinder(Type binderType, Predicate<Type> condition)
        {
            EnsureCanRegisterBinders();

            customBinderRegistrations.Enqueue(
                container =>
                    container.RegisterConditional(typeof(ISafeSettingsBinder<>), binderType, c => condition(c.ServiceType.GetGenericArguments()[0])));
        }

        public void SetupParserFor<T>(ITypeParser parser)
        {
            SetupCustomBinder(new PrimitiveBinder<T>(parser));
        }

        private void EnsureCanRegisterBinders()
        {
            if (containerWrapper.IsValueCreated)
                throw new InvalidOperationException("Cannot register custom binders after the container was used.");
        }
    }
}