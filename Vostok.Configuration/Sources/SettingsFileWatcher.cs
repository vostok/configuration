using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    internal class SingleFileWatcher : IObservable<string>
    {
        private const string DefaultSettingsValue = "\u0001";
        private readonly int watcherPeriod = 5.Seconds().Milliseconds;

        private readonly string filePath;
        private readonly List<IObserver<string>> observers;
        private readonly FileSystemWatcher watcher;
        private string current;
        private Task task;
        private CancellationToken token;
        private CancellationTokenSource tokenSource;
        private static object locker;

        public SingleFileWatcher([NotNull] string filePath)
        {
            this.filePath = filePath;
            observers = new List<IObserver<string>>();
            current = DefaultSettingsValue;
            locker = new object();

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            watcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            lock (locker)
            {
                if (!observers.Contains(observer))
                    observers.Add(observer);
                if (task == null)
                {
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    task = new Task(WachFile, token);
                    task.Start();
                }
            }
            
            return Disposable.Create(
                () =>
                {
                    lock (locker)
                    {
                        if (observers.Contains(observer))
                            observers.Remove(observer);
                        if (observers.Count == 0)
                            Stop();
                    }
                });
        }

        private void Stop() => tokenSource.Cancel();

        private void WachFile()
        {
            while (true)
            {
                if (token.IsCancellationRequested) break;

                try
                {
                    lock (locker)
                        if (CheckFile(out var changes))
                        {
                            foreach (var observer in observers.ToArray())
                                observer.OnNext(changes);
//                            observers.ForEach(o => o.OnNext(changes));
                            current = changes;
                        }
                }
                catch (Exception e)
                {
                    lock (locker)
                    {
                        foreach (var observer in observers.ToArray())
                            observer.OnError(e);
//                        observers.ForEach(o => o.OnError(e));
                    }
                }

                if (token.IsCancellationRequested) break;
                watcher.WaitForChanged(WatcherChangeTypes.All, watcherPeriod);
            }
        }

        private bool CheckFile(out string changes)
        {
            var fileExists = File.Exists(filePath);
            changes = null;

            if (!fileExists && current != null)
                return true;
            else if (fileExists)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    changes = reader.ReadToEnd();

                if (current != changes)
                    return true;
            }

            return false;
        }
    }

    /*/// <summary>
    /// Watchs for changes in files by <see cref="IConfigurationSource"/>.
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private const string DefaultSettingsValue = "\u0001";
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();

        private static ConcurrentDictionary<string, IObserver<string>> observersInfo;
        private static ConcurrentDictionary<IConfigurationSource, FileInfo> filesInfo;
        private static bool needStop;
        private static Thread watcherThread;
        private static object locker;

        /// <summary>
        /// Subscribtion to <paramref name="file" /> for specified <paramref name="source"/>
        /// </summary>
        /// <param name="file">Watching file path</param>
        /// <param name="source">Subscribing source</param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<string> WatchFile(
            [NotNull] string file,
            [NotNull] IConfigurationSource source)
        {
            if (locker == null)
                locker = new object();

            lock (locker)
            {
                if (observersInfo == null)
                    PrepareFileWatcher();
                AddFileInfo(file, source);
            }

            var subscribtion = Observable.Create<string>(
                observer =>
                {
                    lock (locker)
                    {
                        observersInfo.AddOrUpdate(file, observer, (s, o) => observer);
                        if (filesInfo.TryGetValue(source, out var fileInfo) && fileInfo.CurrentValue != DefaultSettingsValue)
                            observer.OnNext(fileInfo.CurrentValue);
                    }

                    return Disposable.Create(
                        () =>
                        {
                            lock (locker)
                                if (observersInfo != null)
                                {
                                    observersInfo.TryRemove(file, out var _);
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
                observersInfo = new ConcurrentDictionary<string, IObserver<string>>();
            if (filesInfo == null)
                filesInfo = new ConcurrentDictionary<IConfigurationSource, FileInfo>();
            needStop = false;

            if (watcherThread == null)
                watcherThread = ThreadRunner.Run(WatchFile);
        }

        private static void AddFileInfo(string filePath, IConfigurationSource source, TimeSpan observationPeriod = default)
        {
            var info = new FileInfo
            {
                FilePath = filePath,
                ObservationPeriod = observationPeriod < MinObservationPeriod ? MinObservationPeriod : observationPeriod,
                NextCheck = DateTime.UtcNow.AddMinutes(-1),
                CurrentValue = DefaultSettingsValue,
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
                        if (observersInfo.TryGetValue(filePath, out var observer))
                            observer?.OnError(e);
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
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    changes = reader.ReadToEnd();

                if (currentValue != changes)
                    return true;
            }

            return false;
        }

        private static void SendChanges(string filePath, string changes)
        {
            foreach (var source in filesInfo.Where(i => i.Value.FilePath == filePath).Select(i => i.Key))
            {
                if (observersInfo.TryGetValue(filePath, out var observer))
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
        }
    }*/
}