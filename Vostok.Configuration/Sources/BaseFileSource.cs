using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    public class BaseFileSource : IConfigurationSource
    {
        protected readonly string FilePath;
        private readonly Func<string, RawSettings> parseSettings;

        /// <summary>
        /// Creating converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="onError">Callback on error</param>
        public BaseFileSource(string filePath, Func<string, RawSettings> parseSettings, TimeSpan observationPeriod = default, Action<Exception> onError = null)
        {
            FilePath = filePath;
            this.parseSettings = parseSettings;
            SettingsFileWatcher.StartSettingsFileWatcher(filePath, this,
                observationPeriod == default ? 10.Seconds() : observationPeriod, onError);
        }

        public RawSettings Get() => 
            !File.Exists(FilePath) ? null : parseSettings(File.ReadAllText(FilePath));

        public IObservable<RawSettings> Observe() => 
            Observable.Create<RawSettings>(observer =>
                SettingsFileWatcher.Subscribe(observer, this));

        public void Dispose() => 
            SettingsFileWatcher.RemoveObservers(this);
    }
}