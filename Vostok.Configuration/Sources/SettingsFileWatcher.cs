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
    /// <summary>
    /// File watcher for settings files
    /// </summary>
    public static class SettingsFileWatcher
    {
        private const int FileWatchingDelay = 100;
        private static List<ObserverInfo> observersInfo;
        private static List<FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;

        /// <summary>
        /// Creating settings file watcher
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="configurationSource">Configuration source for file parsing</param>
        /// <param name="observePeriod">Observe period (min 100)</param>
        /// <param name="callBack">Callback on exception</param>
        public static void StartSettingsFileWatcher(string filePath, IConfigurationSource configurationSource, TimeSpan observePeriod = default, Action<Exception> callBack = null)
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
                ObservePeriond = observePeriod.Milliseconds < 100 ? 100.Milliseconds() : observePeriod,
                ConfigurationSource = configurationSource,
                NextCheck = DateTime.Now.AddMilliseconds(observePeriod.Milliseconds),
                CurrentSettings = null,
                CallBack = callBack,
            });

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }

        /// <summary>
        /// Add subscribtion
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
            var subscribtion = obsInfo.Observers.Where(o => o != null).SubscribeSafe(observer);

            var current = filesInfo.FirstOrDefault(i => i.FilePath == filePath)?.CurrentSettings;
            if (current != null)
                observer.OnNext(current);

            return subscribtion;
        }

        private static void WatchFile()
        {
            while (!needStop)
            {
                Thread.Sleep(FileWatchingDelay);
                if (needStop) break;
                if (observersInfo.Count == 0 || filesInfo == null) continue;

                foreach (var info in filesInfo.Where(i => i.NextCheck <= DateTime.Now))
                    try
                    {
                        CheckFile(info);
                    }
                    catch (Exception e)
                    {
                        info.CallBack?.Invoke(e);
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
                foreach (var observer in observersInfo.Where(i => i.FilePath == filePath))
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
            public TimeSpan ObservePeriond { get; set; }
            public IConfigurationSource ConfigurationSource { get; set; }
            public RawSettings CurrentSettings { get; set; }
            public Action<Exception> CallBack { get; set; }
        }
    }
}