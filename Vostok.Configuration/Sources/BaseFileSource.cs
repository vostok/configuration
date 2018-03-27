using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    public class BaseFileSource : IConfigurationSource
    {
        protected readonly string FilePath;
        private readonly Func<RawSettings> parseSettings;

        /// <summary>
        /// Creating converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        public BaseFileSource(string filePath, Func<RawSettings> parseSettings, TimeSpan observationPeriod = default, Action<Exception> onError = null)
        {
            FilePath = filePath;
            this.parseSettings = parseSettings;
            SettingsFileWatcher.StartSettingsFileWatcher(filePath, this,
                observationPeriod == default ? 10.Seconds() : observationPeriod, onError);
        }

        public RawSettings Get() => 
            !File.Exists(FilePath) ? null : parseSettings(); // CR(krait): parseSettings should be Func<string, RawSettings>. We can put the File.ReadAllText call here.

        public IObservable<RawSettings> Observe() => 
            Observable.Create<RawSettings>(observer =>
                SettingsFileWatcher.Subscribe(observer, FilePath));

        public void Dispose() => 
            SettingsFileWatcher.RemoveObservers(this);
    }
}