using System.Linq;
using Vostok.Commons;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Extensions
{
    // ReSharper disable once InconsistentNaming
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
        /// Creates a <see cref="CombinedSource"/> instance using given parameters <paramref name="source"/>, <paramref name="other"/>, and <paramref name="listCombineOptions"/>
        /// </summary>
        /// <param name="source">First source to combine with</param>
        /// <param name="other">Second source to combine with</param>
        /// <param name="sourceCombineOptions">Options how to combine sources</param>
        /// <param name="combineOptions">Options how to combine sources childs</param>
        /// <returns>A <see cref="CombinedSource"/> instance</returns>
        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other, SourceCombineOptions sourceCombineOptions = SourceCombineOptions.LastIsMain, CombineOptions combineOptions = CombineOptions.Override) => 
            new CombinedSource(new [] {source, other}, sourceCombineOptions, combineOptions);

        /// <summary>
        /// <para>Creates a <see cref="CombinedSource"/> instance using given parameters <paramref name="source"/> and <paramref name="others"/></para>
        /// <para>Uses default list combine option <see cref="ListCombineOptions.FirstOnly"/></para>
        /// </summary>
        /// <param name="source">First source to combine with</param>
        /// <param name="others">Other sources to combine with</param>
        /// <returns>A <see cref="CombinedSource"/> instance</returns>
        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) => 
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());
    }
}