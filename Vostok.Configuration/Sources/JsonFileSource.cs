using System;
using JetBrains.Annotations;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to <see cref="ISettingsNode"/> tree from file
    /// </summary>
    public class JsonFileSource : BaseFileSource
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="JsonFileSource"/> instance using given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        public JsonFileSource([NotNull] string filePath)
            : base(filePath,
                data =>
                {
                    using (var source = new JsonStringSource(data))
                        return source.Get();
                })
        { }

        internal JsonFileSource([NotNull] string filePath, Func<string, IObservable<string>> fileWatcherCreator)
            : base(filePath,
                data =>
                {
                    using (var source = new JsonStringSource(data))
                        return source.Get();
                },
                fileWatcherCreator)
        { }
    }
}