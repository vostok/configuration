using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Extensions
{
    public static class ConfigurationProviderExtensions
    {
        /// <summary>
        /// Configures multiple sources for <see cref="TSettings"/>.
        /// </summary>
        public static ConfigurationProvider SetupSourcesFor<TSettings>(this ConfigurationProvider provider, params IConfigurationSource[] sources) =>
            provider.SetupSourceFor<TSettings>(new CombinedSource(sources));
    }
}