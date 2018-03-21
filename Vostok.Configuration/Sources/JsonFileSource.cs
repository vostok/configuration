﻿using System;
using System.IO;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from file
    /// </summary>
    public class JsonFileSource : IConfigurationSource
    {
        private readonly string filePath;
        private readonly SettingsFileWatcher fileWatcher;

        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100, default 10000)</param>
        public JsonFileSource(string filePath, TimeSpan observePeriod = default)
        {
            this.filePath = filePath;
            fileWatcher = new SettingsFileWatcher(filePath, this,
                observePeriod == default ? TimeSpan.FromMilliseconds(10000) : observePeriod);
        }

        public RawSettings Get()
        {
            if (!File.Exists(filePath)) return null;
            return new JsonStringSource(File.ReadAllText(filePath)).Get();
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                fileWatcher.AddObserver(observer);
                return fileWatcher.GetDisposable(observer);
            });
        }
    }
}