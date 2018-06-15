using System;
using System.Collections.Generic;
using SimpleInjector;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    public delegate bool TryParse<T>(string s, out T value);

    /// <inheritdoc />
    /// <summary>
    /// Default binder
    /// </summary>
    public class DefaultSettingsBinder : ISettingsBinder
    {
        internal readonly Container Container;
        private readonly IDictionary<Type, ITypeParser> parsers;

        /// <summary>
        /// Creates a <see cref="DefaultSettingsBinder"/> instance
        /// </summary>
        public DefaultSettingsBinder()
        {
            parsers = new Dictionary<Type, ITypeParser>();
            
            Container = new Container();
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(PrimitiveAndSimpleBinder<>),
                c =>
                {
                    var type = c.ServiceType.GetGenericArguments()[0];
                    return type.IsPrimitive() || type == typeof(string) || parsers.ContainsKey(type);
                });
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(NullableBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsNullable());
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(EnumBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsEnum);
            Container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            Container.Register(typeof(ISettingsBinder<>), typeof(DictionaryBinder<,>));
            Container.Register(typeof(ISettingsBinder<>), typeof(SetBinder<>));
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ArrayBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsArray);
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ClassAndStructBinder<>),
                c => !c.Handled);
            Container.Register<ISettingsBinderFactory>(() => new SettingsBinderFactory(Container));
            Container.RegisterInstance(typeof(IDictionary<Type, ITypeParser>), parsers);
        }

        /// <inheritdoc />
        /// <summary>
        /// Binds <paramref name="settings"/> tree to type you chose in <typeparamref name="TSettings"/>
        /// </summary>
        /// <typeparam name="TSettings">Type to</typeparam>
        /// <param name="settings">Settings tree</param>
        /// <returns></returns>
        public TSettings Bind<TSettings>(ISettingsNode settings)
        {
            var factory = Container.GetInstance<ISettingsBinderFactory>();
            var binder = factory.CreateFor<TSettings>();
            return binder.Bind(settings);
        }

        /// <summary>
        /// Adds default parsers which can parse from string value to bunch of types
        /// </summary>
        /// <returns>This binder with new default parsers</returns>
        public DefaultSettingsBinder WithDefaultParsers()
        {
            parsers.AddDefaultParsers();
            return this;
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type we need to parse in</typeparam>
        /// <param name="parser">Class with method implemented TryParse&lt;T&gt; delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder WithCustomParser<T>(ITypeParser parser)
        {
            parsers.AddCustomParser<T>(parser);
            return this;
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type we need to parse in</typeparam>
        /// <param name="parseMethod">Method implemented TryParse&lt;T&gt; delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder WithCustomParser<T>(TryParse<T> parseMethod)
        {
            parsers.AddCustomParser(parseMethod);
            return this;
        }
    }
}