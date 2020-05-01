using System;
using System.Collections.Concurrent;
using System.Threading;
using SimpleInjector;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBinderProvider : ISettingsBinderProvider
    {
        private const int CacheCapacity = 1000;

        private static readonly RecyclingBoundedCache<Type, ISafeSettingsBinder<object>> Cache =
            new RecyclingBoundedCache<Type, ISafeSettingsBinder<object>>(CacheCapacity);

        private readonly Lazy<Container> containerWrapper;
        private readonly ConcurrentQueue<Action<Container>> customBinderRegistrationsBefore =
            new ConcurrentQueue<Action<Container>>();
        private readonly ConcurrentQueue<Action<Container>> customBinderRegistrationsAfter =
            new ConcurrentQueue<Action<Container>>();

        public SettingsBinderProvider()
        {
            containerWrapper = new Lazy<Container>(
                () =>
                {
                    var container = new Container();

                    foreach (var registration in customBinderRegistrationsBefore)
                        registration(container);

                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(NullableBinder<>),
                        c => c.ServiceType.GetGenericArguments()[0].IsNullable());
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(EnumBinder<>),
                        c => c.ServiceType.GetGenericArguments()[0].IsEnum);
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(ParseMethodBinder<>),
                        c => !c.Handled && ParseMethodFinder.HasAnyKindOfParseMethod(c.ServiceType.GetGenericArguments()[0]));
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(ListBinder<>));
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(ReadOnlyListBinder<>),
                        c => !c.Handled);
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(DictionaryBinder<,>));
                    container.Register(typeof(ISafeSettingsBinder<>), typeof(SetBinder<>));
                    container.Register(typeof(ISafeSettingsBinder<ISettingsNode>), typeof(IdentityBinder));

                    foreach (var registration in customBinderRegistrationsAfter)
                        registration(container);

                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(InterfaceBinder<>),
                        c => !c.Handled && c.ServiceType.GetGenericArguments()[0].IsInterface);
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(ConstructorBinder<>),
                        c => !c.Handled && ConstructorBinder<object>.CanBeUsedFor(c.ServiceType.GetGenericArguments()[0]));
                    container.RegisterConditional(
                        typeof(ISafeSettingsBinder<>),
                        typeof(ClassStructBinder<>),
                        c => !c.Handled);
                    container.RegisterConditional(
                        typeof(ISettingsBinder<>),
                        typeof(UnsafeBinderWrapper<>),
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
            return Cache.Obtain(
                type,
                t =>
                {
                    if (t.TryObtainBindByBinder<object>(true, out var binder))
                        return binder;

                    return (ISafeSettingsBinder<object>)containerWrapper.Value.GetInstance(typeof(BinderWrapper<>).MakeGenericType(t));
                });
        }

        public void SetupCustomBinder<TValue>(ISafeSettingsBinder<TValue> binder)
        {
            EnsureCanRegisterBinders();

            customBinderRegistrationsBefore.Enqueue(container => container.RegisterInstance(typeof(ISafeSettingsBinder<TValue>), binder));
        }

        public void SetupCustomBinder(Type binderType, Predicate<Type> condition)
        {
            EnsureCanRegisterBinders();

            customBinderRegistrationsBefore.Enqueue(
                container => container.RegisterConditional(
                    typeof(ISettingsBinder<>),
                    binderType,
                    c => condition(c.ServiceType.GetGenericArguments()[0])));
            customBinderRegistrationsAfter.Enqueue(
                container => container.RegisterConditional(
                    typeof(ISafeSettingsBinder<>),
                    typeof(SafeBinderWrapper<>),
                    c =>
                        !c.Handled &&
                        !(c.Consumer?.ImplementationType?.IsClosedTypeOf(typeof(UnsafeBinderWrapper<>)) ?? false) &&
                        condition(c.ServiceType.GetGenericArguments()[0])));
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
