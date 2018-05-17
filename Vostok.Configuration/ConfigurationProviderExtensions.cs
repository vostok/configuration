using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public static class ConfigurationProviderExtensions
    {
        public static ConfigurationProvider SetupSourcesFor<TSettings>(this ConfigurationProvider provider, params IConfigurationSource[] sources) => 
            provider.SetupSourceFor<TSettings>(new CombinedSource(sources));
    }
}