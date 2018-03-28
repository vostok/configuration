using System;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from file
    /// </summary>
    public class JsonFileSource : BaseFileSource
    {
        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="onError">Callback action on exception</param>
        public JsonFileSource(string filePath, TimeSpan observationPeriod = default, Action<Exception> onError = null)
            : base(filePath,
                data =>
                {
                    using (var source = new JsonStringSource(data))
                        return source.Get();
                },
                observationPeriod,
                onError)
        { }
    }
}