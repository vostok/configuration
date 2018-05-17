using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to <see cref="RawSettings"/> tree from file
    /// </summary>
    public class IniFileSource : BaseFileSource
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="T:Vostok.Configuration.Sources.IniFileSource" /> instance using given parameters <paramref name="filePath" />, <paramref name="observationPeriod" />, and <paramref name="onError" />
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="onError">Callback on exception</param>
        public IniFileSource(
            [NotNull] string filePath,
            TimeSpan observationPeriod = default,
            [CanBeNull] Action<Exception> onError = null)
            : base(filePath,
                data =>
                {
                    using (var source = new IniStringSource(data))
                        return source.Get();
                },
                observationPeriod,
                onError)
        { }
    }
}