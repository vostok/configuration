using System;
using System.Reactive.Linq;
using System.Text;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for converters to <see cref="ISettingsNode"/> tree from file
    /// </summary>
    public class BaseFileSource : IConfigurationSource
    {
        private readonly string filePath;
        private readonly FileSourceSettings settings;
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<string, Encoding, IObservable<string>> fileWatcherCreator;
        private readonly TaskSource taskSource;
        private IObservable<ISettingsNode> fileObserver;
        private ISettingsNode currentValue;

        /// <summary>
        /// <para>Creates a <see cref="BaseFileSource"/> instance.</para>
        /// <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        protected BaseFileSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : this(filePath, settings, parseSettings, SettingsFileWatcher.WatchFile)
        {
        }

        internal BaseFileSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings, Func<string, Encoding, IObservable<string>> fileWatcherCreator)
        {
            this.filePath = filePath;
            this.settings = settings ?? new FileSourceSettings();
            this.parseSettings = parseSettings;
            this.fileWatcherCreator = fileWatcherCreator;
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Returns last parsed <see cref="ISettingsNode"/> tree.</para>
        /// <para>Waits for first read.</para>
        /// </summary>
        /// <exception cref="Exception">Only on first read. Otherwise returns last parsed value.</exception>
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="ISettingsNode"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<ISettingsNode> Observe()
        {
            if (fileObserver != null) return fileObserver;

            var fileWatcher = SettingsFileWatcher.WatchFile(filePath, settings.Encoding, fileWatcherCreator);
            fileObserver = fileWatcher.Select(
                str =>
                {
                    currentValue = parseSettings(str);
                    return currentValue;
                });

            return fileObserver;
        }
    }
}