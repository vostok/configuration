using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Extensions
{
    public static class IConfigurationSourceExtensions
    {
        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance using given parameters <paramref name="source"/> and <paramref name="scope"/>
        /// </summary>
        /// <param name="source">Source where to scope in</param>
        /// <param name="scope">Expected scope</param>
        /// <returns>A <see cref="ScopedSource"/> instance</returns>
        public static IConfigurationSource ScopeTo(this IConfigurationSource source, params string[] scope) =>
            new ScopedSource(source, scope);

        /// <summary>
        /// Creates a <see cref="CombinedSource"/> instance using given parameters <paramref name="source"/>, <paramref name="other"/>, and <paramref name="options"/>
        /// </summary>
        /// <param name="source">First source to combine with</param>
        /// <param name="other">Second source to combine with</param>
        /// <param name="options"></param>
        /// <returns>A <see cref="CombinedSource"/> instance</returns>
        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other, SettingsMergeOptions options = null) =>
            new CombinedSource(new[] {source, other}, options);

        /// <summary>
        /// <para>Creates a <see cref="CombinedSource"/> instance using given parameters <paramref name="source"/> and <paramref name="others"/></para>
        /// </summary>
        /// <param name="source">First source to combine with</param>
        /// <param name="others">Other sources to combine with</param>
        /// <returns>A <see cref="CombinedSource"/> instance</returns>
        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) =>
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());
    }
}