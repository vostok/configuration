using System;
using System.Collections.Concurrent;
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
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();

        private static ConcurrentDictionary<IConfigurationSource, BehaviorSubject<RawSettings>> observersInfo;
        private static List<FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;
        private static object sync;

        /// <summary>
        /// Starts settings file watcher
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="configurationSource">Configuration source for file parsing</param>
        /// <param name="observationPeriod">Observe period (min 100)</param>
        /// <param name="callBack">Callback on exception</param>
        public static void StartSettingsFileWatcher(string filePath, IConfigurationSource configurationSource, TimeSpan observationPeriod = default, Action<Exception> callBack = null)
        {
            if (sync == null)
                sync = new object();
            if (observersInfo == null)
                observersInfo = new ConcurrentDictionary<IConfigurationSource, BehaviorSubject<RawSettings>>();
            if (filesInfo == null)
                lock (sync)
                    filesInfo = new List<FileInfo>();
            needStop = false;
            lock (sync)
                filesInfo.Add(new FileInfo
                {
                    FilePath = filePath,
                    LastFileWriteTime = File.GetLastWriteTimeUtc(filePath),
                    ObservationPeriod = observationPeriod < MinObservationPeriod ? MinObservationPeriod : observationPeriod,
                    ConfigurationSource = configurationSource,
                    NextCheck = DateTime.UtcNow + observationPeriod,
                    CurrentSettings = null,
                    OnError = callBack,
                });

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }

        /// <summary>
        /// Add subscription
        /// </summary>
        public static IDisposable Subscribe(IObserver<RawSettings> observer, IConfigurationSource source)
        {
            IDisposable subscription;
            RawSettings current;
            lock (sync)
            {
                BehaviorSubject<RawSettings> obsInfo;
                if (observersInfo.ContainsKey(source))
                    obsInfo = observersInfo[source];
                else
                {
                    obsInfo = new BehaviorSubject<RawSettings>(null);
                    observersInfo.TryAdd(source, obsInfo);
                }
                subscription = obsInfo.Where(o => o != null).SubscribeSafe(observer);

                current = filesInfo.FirstOrDefault(i => i.ConfigurationSource == source)?.CurrentSettings;
            }
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
                FileInfo[] fi;
                lock (sync)
                    fi = filesInfo.Where(i => i.NextCheck <= DateTime.UtcNow).ToArray();
                foreach (var info in fi)
                    try
                    {
                        CheckFile(info);
                        info.NextCheck = DateTime.UtcNow + info.ObservationPeriod;
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
            RawSettings changes = null;
            IConfigurationSource source = null;
            var needReturn = false;

            lock (sync)
            {
                var fileExists = File.Exists(fileInfo.FilePath);
                var lwt = File.GetLastWriteTimeUtc(fileInfo.FilePath);

                if (!fileExists && fileInfo.CurrentSettings != null)
                {
                    fileInfo.LastFileWriteTime = DateTime.UtcNow;
                    source = fileInfo.ConfigurationSource;
                    needReturn = true;
                }
                else if (fileExists && lwt > fileInfo.LastFileWriteTime)
                {
                    fileInfo.LastFileWriteTime = lwt;
                    changes = fileInfo.ConfigurationSource.Get();

                    if (!Equals(fileInfo.CurrentSettings, changes))
                    {
                        source = fileInfo.ConfigurationSource;
                        needReturn = true;
                    }
                }
                if (!needReturn) return;
            }
            
            if (observersInfo.ContainsKey(source))
                observersInfo[source].OnNext(changes);
            FileInfo info;
            lock (sync)
                info = filesInfo.FirstOrDefault(i => i.ConfigurationSource == source);
            if (info != null)
                info.CurrentSettings = changes;
        }

        /// <summary>
        /// Stop watcher and clear all data for next start
        /// </summary>
        public static void StopAndClear()
        {
            lock (sync)
            {
                filesInfo.Clear();
                filesInfo = null;
            }
            needStop = true;
            observersInfo.Values.ToList().ForEach(o => o.Dispose());
            observersInfo.Clear();
            observersInfo = null;
            watcherThread = null;
        }

        /// <summary>
        /// Remove observers of specified source
        /// </summary>
        /// <param name="baseFileSource">Source which observers must be removed</param>
        public static void RemoveObservers(IConfigurationSource baseFileSource)
        {
            if (observersInfo != null && observersInfo.ContainsKey(baseFileSource))
            {
                observersInfo[baseFileSource].Dispose();
                observersInfo.TryRemove(baseFileSource, out var _);
            }
            lock (sync)
                filesInfo?.RemoveAll(i => i.ConfigurationSource == baseFileSource);
        }

        private class FileInfo
        {
            public string FilePath { get; set; }
            public DateTime LastFileWriteTime { get; set; }
            public DateTime NextCheck { get; set; }
            public TimeSpan ObservationPeriod { get; set; }
            public IConfigurationSource ConfigurationSource { get; set; }
            public RawSettings CurrentSettings { get; set; }
            public Action<Exception> OnError { get; set; }
        }
    }
}