using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to <see cref="RawSettings"/> tree from file
    /// </summary>
    public class JsonFileSource : BaseFileSource
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="JsonFileSource"/> instance using given parameters <paramref name="filePath"/> and <paramref name="observationPeriod"/>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        public JsonFileSource(
            [NotNull] string filePath)
            : base(filePath,
                data =>
                {
                    using (var source = new JsonStringSource(data))
                        return source.Get();
                })
        { }
    }
}