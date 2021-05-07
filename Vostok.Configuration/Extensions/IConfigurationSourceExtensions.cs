using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Extensions
{
    [PublicAPI]
    public static class IConfigurationSourceExtensions
    {
        [CanBeNull]
        public static ISettingsNode Get([NotNull] this IConfigurationSource source)
        {
            var providerSettings = new ConfigurationProviderSettings
            {
                Binder = new IdentityBinder()
            };

            using (var provider = new ConfigurationProvider(providerSettings))
                return provider.Get<ISettingsNode>(source);
        }

        public static void ApplyTo<TSettings>([NotNull] this IConfigurationSource source, [NotNull] TSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var node = source.Get();
            if (node == null)
                return;

            using (ClassStructBinderSeed.Use(node, settings))
                new SettingsBinderProvider().CreateFor<TSettings>().Bind(node).EnsureSuccess();
        }
    }
}