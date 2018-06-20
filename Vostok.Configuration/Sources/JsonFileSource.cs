using System;
using System.Text;
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
        /// <param name="settings">File parsing settings</param>
        public JsonFileSource([NotNull] string filePath, FileSourceSettings settings = null)
            : base(filePath, settings, ParseSettings)
        {
        }

        internal JsonFileSource([NotNull] string filePath, Func<string, Encoding, IObservable<string>> fileWatcherCreator, FileSourceSettings settings = null)
            : base(filePath, settings, ParseSettings, fileWatcherCreator)
        {
        }

        private static ISettingsNode ParseSettings(string str) => new JsonStringSource(str).Get();
    }
}