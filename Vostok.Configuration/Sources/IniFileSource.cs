using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to <see cref="ISettingsNode"/> tree from file
    /// </summary>
    public class IniFileSource : BaseFileSource
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="T:Vostok.Configuration.Sources.IniFileSource" /> instance using given parameter <paramref name="filePath" />
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        public IniFileSource([NotNull] string filePath)
            : base(filePath, ParseSettings)
        {
        }

        internal IniFileSource([NotNull] string filePath, Func<string, IObservable<string>> fileWatcherCreator)
            : base(filePath, ParseSettings, fileWatcherCreator)
        {
        }

        private static ISettingsNode ParseSettings(string str) => new IniStringSource(str).Get();
    }
}