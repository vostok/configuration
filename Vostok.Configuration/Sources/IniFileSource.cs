using System;
using System.IO;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to RawSettings tree from file
    /// </summary>
    public class IniFileSource : BaseFileSource
    {
        /// <summary>
        /// Creating ini converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="onError">Callback on exception</param>
        public IniFileSource(string filePath, TimeSpan observationPeriod = default, Action<Exception> onError = null)
            : base(filePath,
                () =>
                {
                    using (var source = new IniStringSource(File.ReadAllText(filePath)))
                        return source.Get();
                },
                observationPeriod,
                onError)
        { }
    }
}