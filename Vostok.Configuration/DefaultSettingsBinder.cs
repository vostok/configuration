using System;
using SimpleInjector;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration
{
    public delegate bool TryParse<T>(string s, out T value);
    public delegate bool TryBind(RawSettings settings, Type bindType, out object result);

    /// <inheritdoc />
    /// <summary>
    /// Default binder
    /// </summary>
    public class DefaultSettingsBinder : ISettingsBinder
    {
        private readonly Container container;

        /// <summary>
        /// Creates a <see cref="DefaultSettingsBinder"/> instance
        /// </summary>
        public DefaultSettingsBinder()
        {
            container = new Container();
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(PrimitiveAndSimpleBinder<>),
                c => PrimitiveAndSimpleBinder<bool>.IsAvailableType(c.ServiceType.GetGenericArguments()[0]));
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(NullableBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsNullable());
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(EnumBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsEnum);
            container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            container.Register(typeof(ISettingsBinder<>), typeof(DictionaryBinder<,>));
            container.Register(typeof(ISettingsBinder<>), typeof(SetBinder<>));
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ArrayBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsArray);
            container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ClassAndStructBinder<>),
                c =>
                {
                    var type = c.ServiceType.GetGenericArguments()[0];
                    return type.IsValueType && !type.IsPrimitive && !type.IsGenericType && !type.IsEnum && !PrimitiveAndSimpleBinder<bool>.IsAvailableType(type) || !c.Handled;
                });
            container.Register<ISettingsBinderFactory>(() => new SettingsBinderFactory(container));
        }

        /// <inheritdoc />
        /// <summary>
        /// Binds <paramref name="settings"/> tree to type you chose in <typeparamref name="TSettings"/>
        /// </summary>
        /// <typeparam name="TSettings">Type to</typeparam>
        /// <param name="settings">Settings tree</param>
        /// <returns></returns>
        public TSettings Bind<TSettings>(RawSettings settings)
        {
            var factory = container.GetInstance<ISettingsBinderFactory>();
            var binder = factory.CreateFor<TSettings>();
            return binder.Bind(settings);
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type we need to parse in</typeparam>
        /// <param name="parser">Class with method implemented TryParse&lt;T&gt; delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder AddCustomParser<T>(ITypeParser parser)
        {
            PrimitiveAndSimpleParsers.AddCustomParser<T>(parser);
            return this;
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type we need to parse in</typeparam>
        /// <param name="parseMethod">Method implemented TryParse&lt;T&gt; delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder AddCustomParser<T>(TryParse<T> parseMethod)
        {
            PrimitiveAndSimpleParsers.AddCustomParser(parseMethod);
            return this;
        }
    }
}