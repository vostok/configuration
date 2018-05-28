using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for converters to <see cref="IRawSettings"/> tree from file
    /// </summary>
    public class BaseFileSource : IConfigurationSource
    {
        private readonly string filePath;
        private readonly Func<string, IRawSettings> parseSettings;
        private readonly TaskSource taskSource;
        private IObservable<IRawSettings> fileObserver;
        private IRawSettings currentValue;

        /// <summary>
        /// <para>Creates a <see cref="BaseFileSource"/> instance.</para>
        /// <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        protected BaseFileSource(string filePath, Func<string, IRawSettings> parseSettings)
        {
            this.filePath = filePath;
            this.parseSettings = parseSettings;
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Returns last parsed <see cref="IRawSettings"/> tree.</para>
        /// <para>Waits for first read.</para>
        /// </summary>
        /// <exception cref="Exception">Only on first read. Otherwise returns last parsed value.</exception>
        public IRawSettings Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="IRawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe()
        {
            if (fileObserver != null) return fileObserver;

            var fileWatcher = SettingsFileWatcher.WatchFile(filePath);
            fileObserver = fileWatcher.Select(
                str =>
                {
                    currentValue = parseSettings(str);
                    return currentValue;
                });

            return fileObserver;
        }

        public void Dispose()
        {
        }
    }
}