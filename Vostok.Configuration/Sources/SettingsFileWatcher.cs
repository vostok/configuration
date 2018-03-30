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
        private static ConcurrentDictionary<IConfigurationSource, FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;

        /// <summary>
        /// Starts settings file watcher
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="configurationSource">Configuration source for file parsing</param>
        /// <param name="observationPeriod">Observe period (min 100)</param>
        /// <param name="callBack">Callback on exception</param>
        public static void StartSettingsFileWatcher(string filePath, IConfigurationSource configurationSource, TimeSpan observationPeriod = default, Action<Exception> callBack = null)
        {
            if (observersInfo == null)
                observersInfo = new ConcurrentDictionary<IConfigurationSource, BehaviorSubject<RawSettings>>();
            if (filesInfo == null)
                filesInfo = new ConcurrentDictionary<IConfigurationSource, FileInfo>();
            needStop = false;
            var info = new FileInfo
            {
                FilePath = filePath,
                LastFileWriteTime = File.GetLastWriteTimeUtc(filePath),
                ObservationPeriod = observationPeriod < MinObservationPeriod ? MinObservationPeriod : observationPeriod,
                NextCheck = DateTime.UtcNow + observationPeriod,
                CurrentSettings = null,
                OnError = callBack,
            };
            filesInfo.AddOrUpdate(configurationSource, info, (cs, fi) => info);

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }

        /// <summary>
        /// Add subscription
        /// </summary>
        public static IDisposable Subscribe(IObserver<RawSettings> observer, IConfigurationSource source)
        {
            RawSettings current = null;
            BehaviorSubject<RawSettings> obsInfo;
            if (observersInfo.ContainsKey(source))
                obsInfo = observersInfo[source];
            else
            {
                obsInfo = new BehaviorSubject<RawSettings>(null);
                observersInfo.TryAdd(source, obsInfo);
            }
            var subscription = obsInfo.Where(o => o != null).SubscribeSafe(observer);

            if (filesInfo.TryGetValue(source, out var info))
                current = info?.CurrentSettings;
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
                foreach (var pair in filesInfo.Where(i => i.Value.NextCheck <= DateTime.UtcNow))
                    try
                    {
                        CheckFile(pair);
                        pair.Value.NextCheck = DateTime.UtcNow + pair.Value.ObservationPeriod;
                    }
                    catch (Exception e)
                    {
                        pair.Value.OnError?.Invoke(e);
                    }
            }
            needStop = false;
        }

        private static void CheckFile(KeyValuePair<IConfigurationSource, FileInfo> pair)
        {
            RawSettings changes = null;
            IConfigurationSource source = null;
            var needReturn = false;
            var fileExists = File.Exists(pair.Value.FilePath);
            var lwt = File.GetLastWriteTimeUtc(pair.Value.FilePath);

            if (!fileExists && pair.Value.CurrentSettings != null)
            {
                pair.Value.LastFileWriteTime = DateTime.UtcNow;
                source = pair.Key;
                needReturn = true;
            }
            else if (fileExists && (lwt > pair.Value.LastFileWriteTime || pair.Value.CurrentSettings == null))
            {
                pair.Value.LastFileWriteTime = lwt;
                changes = pair.Key.Get();

                if (!Equals(pair.Value.CurrentSettings, changes))
                {
                    source = pair.Key;
                    needReturn = true;
                }
            }
            if (!needReturn) return;

            if (observersInfo.ContainsKey(source))
                observersInfo[source].OnNext(changes);
            if (filesInfo.TryGetValue(pair.Key, out var info) && info != null)
                info.CurrentSettings = changes;
        }

        /// <summary>
        /// Stop watcher and clear all data for next start
        /// </summary>
        public static void StopAndClear()
        {
            filesInfo.Clear();
            filesInfo = null;
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
            filesInfo?.TryRemove(baseFileSource, out var _);
        }

        private class FileInfo
        {
            public string FilePath { get; set; }
            public DateTime LastFileWriteTime { get; set; }
            public DateTime NextCheck { get; set; }
            public TimeSpan ObservationPeriod { get; set; }
            public RawSettings CurrentSettings { get; set; }
            public Action<Exception> OnError { get; set; }
        }
    }
}