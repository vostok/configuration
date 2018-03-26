using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    public class BaseFileSource : IConfigurationSource
    {
        protected readonly string FilePath;
        private readonly Func<RawSettings> getSource;

        /// <summary>
        /// Creating converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="getSource">"Get" method invocation for string source</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        public BaseFileSource(string filePath, Func<RawSettings> getSource, TimeSpan observePeriod = default, Action<Exception> callBack = null)
        {
            FilePath = filePath;
            this.getSource = getSource;
            SettingsFileWatcher.StartSettingsFileWatcher(filePath, this,
                observePeriod == default ? 10.Seconds() : observePeriod, callBack);
        }

        public RawSettings Get() => 
            !File.Exists(FilePath) ? null : getSource();

        public IObservable<RawSettings> Observe() => 
            Observable.Create<RawSettings>(observer =>
                SettingsFileWatcher.Subscribe(observer, FilePath));

        public void Dispose() => 
            SettingsFileWatcher.RemoveObservers(this);
    }
}