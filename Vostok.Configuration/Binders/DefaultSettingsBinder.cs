using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
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

        public TSettings Bind<TSettings>(ISettingsNode settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var binder = binderProvider.CreateFor<TSettings>();
            return binder.Bind(settings);
        }

        public DefaultSettingsBinder WithParserFor<T>(TryParse<T> parser)
        {
            binderProvider.WithParserFor(parser);
            return this;
        }
    }
}