using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    public class BaseFileSource<TStringSource> : IConfigurationSource
        where TStringSource : IConfigurationSource
    {
        protected readonly string FilePath;
        private readonly SettingsFileWatcher fileWatcher;

        /// <summary>
        /// Creating converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        public BaseFileSource(string filePath, TimeSpan observePeriod = default)
        {
            FilePath = filePath;
            fileWatcher = new SettingsFileWatcher(filePath, this,
                observePeriod == default ? 10.Seconds() : observePeriod);
        }

        public RawSettings Get()
        {
            if (!File.Exists(FilePath)) return null;
            using (var inst = (TStringSource)Activator.CreateInstance(typeof(TStringSource), File.ReadAllText(FilePath)))
                return inst.Get();
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                fileWatcher.AddObserver(observer);
                return fileWatcher.GetDisposable(observer);
            });
        }

        public void Dispose()
        {
            fileWatcher?.Dispose();
        }
    }
}
