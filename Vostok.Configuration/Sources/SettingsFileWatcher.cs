using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
        private static object sync;
        private static List<ObserverInfo> observers;
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
            if (observers == null)
                observers = new List<ObserverInfo>();
            if (sync == null)
                sync = new object();
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
        /// Add new observer to observers list
        /// </summary>
        public static void AddObserver(IObserver<RawSettings> observer, string filePath)
        {
            lock (sync)
            {
                observers.Add(new ObserverInfo
                {
                    Observer = observer,
                    FilePath = filePath,
                });

                var info = filesInfo.FirstOrDefault(c => c.FilePath == filePath);
                if (info?.CurrentSettings != null)
                    observer.OnNext(info.CurrentSettings);
            }
        }

        /// <summary>
        /// Returns IDisposable for creation IObservable instance
        /// </summary>
        public static IDisposable GetDisposable(IObserver<RawSettings> observer)
        {
            return Disposable.Create(() =>
            {
                lock (sync)
                    observers.RemoveAll(o => o.Observer == observer);
            });
        }

        private static void WatchFile()
        {
            while (!needStop)
            {
                Thread.Sleep(100);
                if (needStop) break;
                if (observers.Count == 0 || filesInfo == null) continue;

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
                LockedReturn(null, fileInfo.FilePath);
            }
            else if (fileExists && lwt > fileInfo.LastFileWriteTime)
            {
                fileInfo.LastFileWriteTime = lwt;
                var changes = fileInfo.ConfigurationSource.Get();

                if (!Equals(fileInfo.CurrentSettings, changes))
                    LockedReturn(changes, fileInfo.FilePath);
            }

            void LockedReturn(RawSettings changes, string filePath)
            {
                if (sync == null) return;
                lock (sync)
                {
                    foreach (var observer in observers.Where(o => o.FilePath == filePath))
                        observer.Observer.OnNext(changes);
                    var info = filesInfo.FirstOrDefault(i => i.FilePath == filePath);
                    if (info != null)
                        info.CurrentSettings = changes;
                }
            }
        }

        public static void StopAndClear()
        {
            needStop = true;
            filesInfo.Clear();
            filesInfo = null;
            lock (sync)
            {
                observers.Clear();
                observers = null;
            }
            watcherThread = null;
            sync = null;
        }

        public static void RemoveObservers(IConfigurationSource baseFileSource)
        {
            var filePath = filesInfo?.FirstOrDefault(fi => fi.ConfigurationSource == baseFileSource)?.FilePath;
            lock (sync)
                observers?.RemoveAll(o => o.FilePath == filePath);
            filesInfo?.RemoveAll(o => o.FilePath == filePath);
        }

        private class ObserverInfo
        {
            public string FilePath { get; set; }
            public IObserver<RawSettings> Observer { get; set; }
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