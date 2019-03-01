using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    /// <summary>
    /// <para>Default implementation of <see cref="ISettingsBinder"/>.</para>
    /// <para>Can be configured with parsers for custom simple types via <see cref="WithParserFor{T}"/>.</para>
    /// </summary>
    [PublicAPI]
    public class DefaultSettingsBinder : ISettingsBinder
    {
        private readonly ISettingsBinderProvider binderProvider;

        /// <summary>
        /// Creates a <see cref="DefaultSettingsBinder"/> instance.
        /// </summary>
        public DefaultSettingsBinder()
            : this(new SettingsBinderProvider())
        {
        }

        internal DefaultSettingsBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider.WithDefaultParsers();

        /// <inheritdoc />
        public TSettings Bind<TSettings>(ISettingsNode settings)
        {
            var binder = binderProvider.CreateFor<TSettings>();
            return binder.Bind(settings).Value;
        }

        /// <summary>
        /// Adds a new simple type <typeparamref name="T"/> with the given parser.
        /// </summary>
        public DefaultSettingsBinder WithParserFor<T>(TryParse<T> parser)
        {
            binderProvider.WithParserFor(parser);
            return this;
        }

        /// <summary>
        /// Adds a new binder for type <typeparamref name="TValue"/>. For generic binders use the <see cref="WithCustomBinder(Type,Predicate{Type})"/> overload.
        /// </summary>
        public DefaultSettingsBinder WithCustomBinder<TValue>(ISettingsBinder<TValue> binder)
        {
            binderProvider.SetupCustomBinder(new SafeBinderWrapper<TValue>(binder));
            return this;
        }

        /// <summary>
        /// <para>Adds a new binder of generic type.</para>
        /// <para>The provided <paramref name="binderType"/> should be an open generic type, i.e. <c>typeof(MyBinder&lt;&gt;)</c>.</para>
        /// <para>The <paramref name="condition"/> predicate is used to select which service types should be handled with this binder.</para> 
        /// </summary>
        public DefaultSettingsBinder WithCustomBinder(Type binderType, Predicate<Type> condition)
        {
            binderProvider.SetupCustomBinder(binderType, condition);
            return this;
        }
    }
}