using System.Linq;
using Vostok.Commons;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public static class Extensions
    {
        public static IConfigurationSource ScopeTo(this IConfigurationSource source, params string[] scope) => 
            new ScopedSource(source, scope);

        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other) => 
            new CombinedSource(source, other);

        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) => 
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());

        public static ConfigurationProvider WithSourcesFor<TSettings>(this ConfigurationProvider provider, params IConfigurationSource[] sources) => 
            provider.WithSourceFor<TSettings>(new CombinedSource(sources));
    }
}