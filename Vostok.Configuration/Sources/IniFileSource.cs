/*using JetBrains.Annotations;

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
        /// Creates a <see cref="T:Vostok.Configuration.Sources.IniFileSource" /> instance using given parameter <paramref name="filePath" />
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        public IniFileSource([NotNull] string filePath)
            : base(filePath,
                data =>
                {
                    using (var source = new IniStringSource(data))
                        return source.Get();
                })
        { }
    }
}*/