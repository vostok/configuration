using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;
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
            return binder.Bind(settings);
        }

        /// <summary>
        /// Adds a new simple type <typeparamref name="T"/> with the given parser.
        /// </summary>
        public DefaultSettingsBinder WithParserFor<T>(TryParse<T> parser)
        {
            binderProvider.WithParserFor(parser);
            return this;
        }
    }
}