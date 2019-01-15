using System;
using System.Collections.Generic;
using SimpleInjector;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Helpers;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBinderProvider : ISettingsBinderProvider
    {
        private readonly Container container;
        private readonly Dictionary<Type, ITypeParser> parsers;

        public SettingsBinderProvider()
        {
            parsers = new Dictionary<Type, ITypeParser>();

            container = new Container();
            container.RegisterConditional(
                typeof(ISettingsBinder<>),
                typeof(PrimitiveBinder<>),
                c => parsers.ContainsKey(c.ServiceType.GetGenericArguments()[0]));
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
            container.RegisterInstance(typeof(IDictionary<Type, ITypeParser>), parsers);
        }

        public ISettingsBinder<T> CreateFor<T>() =>
            container.GetInstance<ISettingsBinder<T>>();

        public ISettingsBinder<object> CreateFor(Type type) =>
            (ISettingsBinder<object>)container.GetInstance(typeof(BinderWrapper<>).MakeGenericType(type));

        public void SetupParserFor<T>(ITypeParser parser) => parsers[typeof(T)] = parser;
    }
}