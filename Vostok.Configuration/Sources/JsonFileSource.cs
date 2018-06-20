using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

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
            : base(filePath, ParseSettings)
        {
        }

        internal JsonFileSource([NotNull] string filePath, Func<string, IObservable<string>> fileWatcherCreator)
            : base(filePath, ParseSettings, fileWatcherCreator)
        {
        }

        private static ISettingsNode ParseSettings(string str) => new JsonStringSource(str).Get();
    }
}