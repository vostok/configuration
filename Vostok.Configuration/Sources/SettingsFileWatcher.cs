using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Watchs for changes in files by <see cref="IConfigurationSource"/>.
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();
        private const string DefaultSettingsValue = "\u0001";

        private static ConcurrentDictionary<IConfigurationSource, IObserver<string>> observersInfo;
        private static ConcurrentDictionary<IConfigurationSource, FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;
        private static object locker;

        /// <summary>
        /// Subscribtion to <paramref name="file" /> for specified <paramref name="source"/> with <paramref name="observationPeriod"/>
        /// </summary>
        /// <param name="file">Watching file path</param>
        /// <param name="source">Subscribing source</param>
        /// <param name="observationPeriod">Observation period for <paramref name="file"/></param>
        /// <param name="callBack">Callback in case of any exception while reading the file</param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<string> WatchFile(
            [NotNull] string file,
            [NotNull] IConfigurationSource source,
            TimeSpan observationPeriod = default,
            [CanBeNull] Action<Exception> callBack = null)
        {
            if (locker == null)
                locker = new object();

            lock (locker)
            {
                if (observersInfo == null)
                    PrepareFileWatcher();
                AddFileInfo(file, source, observationPeriod, callBack);
            }

            var subscribtion = Observable.Create<string>(
                observer =>
                {
                    lock (locker)
                    {
                        observersInfo.AddOrUpdate(source, observer, (s, o) => observer);
                        if (filesInfo.TryGetValue(source, out var fileInfo) && fileInfo.CurrentValue != DefaultSettingsValue)
                            observer.OnNext(fileInfo.CurrentValue);
                    }
                    
                    return Disposable.Create(
                        () =>
                        {
                            lock (locker)
                            {
                                observersInfo.TryRemove(source, out var _);
                                if (!observersInfo.Any())
                                    StopAndClear();
                            }
                        });
                });

            lock (locker)

            return subscribtion;
        }

        private static void PrepareFileWatcher()
        {
            if (observersInfo == null)
                observersInfo = new ConcurrentDictionary<IConfigurationSource, IObserver<string>>();
            if (filesInfo == null)
                filesInfo = new ConcurrentDictionary<IConfigurationSource, FileInfo>();
            needStop = false;

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }
        
        private static void AddFileInfo(string filePath, IConfigurationSource source, TimeSpan observationPeriod = default, Action<Exception> callBack = null)
        {
            var info = new FileInfo
            {
                FilePath = filePath,
                ObservationPeriod = observationPeriod < MinObservationPeriod ? MinObservationPeriod : observationPeriod,
                NextCheck = DateTime.UtcNow.AddMinutes(-1),
                CurrentValue = DefaultSettingsValue,
                OnError = callBack,
            };
            filesInfo.AddOrUpdate(source, info, (cs, fi) => info);
        }

        private static void WatchFile()
        {
            while (!needStop)
            {
                Thread.Sleep(MinObservationPeriod);

                if (observersInfo.Count == 0) continue;
                var needCheck = filesInfo.Where(i => i.Value.NextCheck <= DateTime.UtcNow).ToArray();
                foreach (var (filePath, currentValue) in needCheck.Select(p => (p.Value.FilePath, p.Value.CurrentValue)).Distinct())
                {
                    var filesByPath = filesInfo.Where(n => n.Value.FilePath == filePath).ToArray();
                    try
                    {
                        if (CheckFile(filePath, currentValue, out var changes))
                            SendChanges(filePath, changes);
                        var time = DateTime.UtcNow;
                        foreach (var file in filesByPath)
                            file.Value.NextCheck = time + file.Value.ObservationPeriod;
                    }
                    catch (Exception e)
                    {
                        foreach (var file in filesByPath)
                            file.Value.OnError?.Invoke(e);
                    }
                }
            }

            needStop = false;
        }

        private static bool CheckFile(string filePath, string currentValue, out string changes)
        {
            var fileExists = File.Exists(filePath);
            changes = null;

            if (!fileExists && currentValue != null)
                return true;
            else if (fileExists)
            {
                changes = File.ReadAllText(filePath);

                if (currentValue != changes)
                    return true;
            }

            return false;
        }

        private static void SendChanges(string filePath, string changes)
        {
            foreach (var source in filesInfo.Where(i => i.Value.FilePath == filePath).Select(i => i.Key))
            {
                if (observersInfo.TryGetValue(source, out var observer))
                    observer?.OnNext(changes);
                if (filesInfo.TryGetValue(source, out var info) && info != null)
                    info.CurrentValue = changes;
            }
        }

        private static void StopAndClear()
        {
            filesInfo.Clear();
            filesInfo = null;
            needStop = true;
            observersInfo.Clear();
            observersInfo = null;
            watcherThread = null;
        }

        private class FileInfo
        {
            public string FilePath { get; set; }
            public DateTime NextCheck { get; set; }
            public TimeSpan ObservationPeriod { get; set; }
            public string CurrentValue { get; set; }
            public Action<Exception> OnError { get; set; }
        }
    }
}