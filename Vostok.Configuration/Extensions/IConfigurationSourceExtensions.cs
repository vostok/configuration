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
    }
}