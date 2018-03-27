using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    // CR(krait): It must be thread-safe, obviously.
    /// <summary>
    /// File watcher for settings files
    /// </summary>
    public static class SettingsFileWatcher
    {
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();

        private static List<ObserverInfo> observersInfo;
        private static List<FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;

        /// <summary>
        /// Creating settings file watcher
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="configurationSource">Configuration source for file parsing</param>
        /// <param name="observationPeriod">Observe period (min 100)</param>
        /// <param name="callBack">Callback on exception</param>
        public static void StartSettingsFileWatcher(string filePath, IConfigurationSource configurationSource, TimeSpan observationPeriod = default, Action<Exception> callBack = null)
        {
            if (observersInfo == null)
                observersInfo = new List<ObserverInfo>();
            if (filesInfo == null)
                filesInfo = new List<FileInfo>();
            needStop = false;
            filesInfo.Add(new FileInfo
            {
                FilePath = filePath,
                LastFileWriteTime = File.GetLastWriteTimeUtc(filePath),
                ObservationPeriod = observationPeriod < MinObservationPeriod ? MinObservationPeriod : observationPeriod,
                ConfigurationSource = configurationSource,
                NextCheck = DateTime.Now + observationPeriod, // CR(krait): DateTime.Now should never be used. Use DateTime.UtcNow instead: https://blogs.msdn.microsoft.com/kirillosenkov/2012/01/10/datetime-utcnow-is-generally-preferable-to-datetime-now/
                CurrentSettings = null,
                OnError = callBack,
            });

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }

        /// <summary>
        /// Add subscription
        /// </summary>
        public static IDisposable Subscribe(IObserver<RawSettings> observer, string filePath)
        {
            var obsInfo = observersInfo.FirstOrDefault(i => i.FilePath == filePath);
            if (obsInfo == null)
            {
                obsInfo = new ObserverInfo
                {
                    FilePath = filePath,
                    Observers = new BehaviorSubject<RawSettings>(null),
                };
                observersInfo.Add(obsInfo);
            }
            var subscription = obsInfo.Observers.Where(o => o != null).SubscribeSafe(observer);

            var current = filesInfo.FirstOrDefault(i => i.FilePath == filePath)?.CurrentSettings;
            if (current != null)
                observer.OnNext(current);

            return subscription;
        }

        private static void WatchFile()
        {
            while (!needStop)
            {
                Thread.Sleep(MinObservationPeriod);
                if (needStop) break;
                if (observersInfo.Count == 0 || filesInfo == null) continue;

                foreach (var info in filesInfo.Where(i => i.NextCheck <= DateTime.Now))
                    try
                    {
                        CheckFile(info);
                    }
                    catch (Exception e)
                    {
                        info.OnError?.Invoke(e);
                    }
            }
            needStop = false;
        }

        private static void CheckFile(FileInfo fileInfo)
        {
            var fileExists = File.Exists(fileInfo.FilePath);
            var lwt = File.GetLastWriteTimeUtc(fileInfo.FilePath);

            if (!fileExists && fileInfo.CurrentSettings != null)
            {
                fileInfo.LastFileWriteTime = DateTime.UtcNow;
                Return(null, fileInfo.FilePath);
            }
            else if (fileExists && lwt > fileInfo.LastFileWriteTime)
            {
                fileInfo.LastFileWriteTime = lwt;
                var changes = fileInfo.ConfigurationSource.Get();

                if (!Equals(fileInfo.CurrentSettings, changes))
                    Return(changes, fileInfo.FilePath);
            }

            void Return(RawSettings changes, string filePath)
            {
                foreach (var observer in observersInfo.Where(i => i.FilePath == filePath)) // CR(krait): Why not just store them already grouped by file path?
                    observer.Observers.OnNext(changes);
                var info = filesInfo.FirstOrDefault(i => i.FilePath == filePath);
                if (info != null)
                    info.CurrentSettings = changes;
            }
        }

        public static void StopAndClear()
        {
            needStop = true;
            filesInfo.Clear();
            filesInfo = null;
            observersInfo.ForEach(i => i.Observers.Dispose());
            observersInfo.Clear();
            observersInfo = null;
            watcherThread = null;
        }

        public static void RemoveObservers(IConfigurationSource baseFileSource)
        {
            var filePath = filesInfo?.FirstOrDefault(fi => fi.ConfigurationSource == baseFileSource)?.FilePath;
            foreach (var obsInfo in observersInfo.Where(i => i.FilePath == filePath))
                obsInfo.Observers.Dispose();
            observersInfo?.RemoveAll(i => i.FilePath == filePath);
            filesInfo?.RemoveAll(i => i.FilePath == filePath);
        }

        private class ObserverInfo
        {
            public string FilePath { get; set; }
            public BehaviorSubject<RawSettings> Observers { get; set; }
        }

        private class FileInfo
        {
            public string FilePath { get; set; }
            public DateTime LastFileWriteTime { get; set; }
            public DateTime NextCheck { get; set; }
            public TimeSpan ObservationPeriod { get; set; }
            public IConfigurationSource ConfigurationSource { get; set; } // CR(krait): Why IConfigurationSource? Just pass the Get() method.
            public RawSettings CurrentSettings { get; set; }
            public Action<Exception> OnError { get; set; }
        }
    }
}