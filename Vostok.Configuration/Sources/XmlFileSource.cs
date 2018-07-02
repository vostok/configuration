using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Xml converter to <see cref="ISettingsNode"/> tree from file
    /// </summary>
    public class XmlFileSource : BaseFileSource
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="XmlFileSource"/> instance using given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        public XmlFileSource([NotNull] string filePath, FileSourceSettings settings = null)
            : base(filePath, settings, ParseSettings)
        {
        }

        internal XmlFileSource([NotNull] string filePath, Func<string, FileSourceSettings, IObservable<string>> fileWatcherCreator, FileSourceSettings settings = null)
            : base(filePath, settings, ParseSettings, fileWatcherCreator)
        {
        }

        private static ISettingsNode ParseSettings(string str) => new XmlStringSource(str).Get();
    }
}