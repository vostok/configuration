using System.Linq;
using Vostok.Commons;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public static class IConfigurationSourceExtensions
    {
        public static IConfigurationSource ScopeTo(this IConfigurationSource source, params string[] scope) => 
            new ScopedSource(source, scope);

        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other, ListCombineOptions listCombineOptions = ListCombineOptions.FirstOnly) => 
            new CombinedSource(new [] {source, other}, listCombineOptions);

        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) => 
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());
    }
}