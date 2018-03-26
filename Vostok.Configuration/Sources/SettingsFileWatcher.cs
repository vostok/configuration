using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// File watcher for settings files
    /// </summary>
    internal class SettingsFileWatcher: IDisposable
    {
        private readonly string filePath;
        private readonly IConfigurationSource configurationSource;
        private readonly FileSystemWatcher watcher;
        private readonly List<IObserver<RawSettings>> observers;
        private readonly object sync;
        private readonly TimeSpan observePeriod;
        private DateTime lastFileWriteTime;
        private RawSettings current;
        private bool disposing;

        /// <summary>
        /// Creating settings file watcher
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="configurationSource">Configuration source for file parsing</param>
        /// <param name="observePeriod">Observe period (min 100)</param>
        public SettingsFileWatcher(string filePath, IConfigurationSource configurationSource, TimeSpan observePeriod = default)
        {
            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.filePath = filePath;
            this.configurationSource = configurationSource;
            watcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            observers = new List<IObserver<RawSettings>>();
            sync = new object();
            this.observePeriod = observePeriod.Milliseconds < 100 ? TimeSpan.FromMilliseconds(100) : observePeriod;
            lastFileWriteTime = File.GetLastWriteTimeUtc(filePath);
            disposing = false;

            ThreadRunner.Run(WatchFile);
        }

        /// <summary>
        /// Add new observer to observers list
        /// </summary>
        public void AddObserver(IObserver<RawSettings> observer)
        {
            lock (sync)
            {
                observers.Add(observer);

                if (current != null)
                    observer.OnNext(current);
            }
        }

        /// <summary>
        /// Returns IDisposable for creation IObservable instance
        /// </summary>
        public IDisposable GetDisposable(IObserver<RawSettings> observer)
        {
            return Disposable.Create(() =>
            {
                lock (sync)
                {
                    observers.Remove(observer);
                }
            });
        }

        private void WatchFile()
        {
            void LockedReturn(RawSettings changes)
            {
                lock (sync)
                {
                    foreach (var observer in observers)
                        observer.OnNext(changes);
                    current = changes;
                }
            }

            while (!disposing)
            {
                watcher.WaitForChanged(WatcherChangeTypes.All, observePeriod.Milliseconds);
                if (disposing) break;
                if (observers.Count == 0) continue;

                var fileExists = File.Exists(filePath);
                var lwt = File.GetLastWriteTimeUtc(filePath);

                if (!fileExists && current != null)
                {
                    lastFileWriteTime = DateTime.UtcNow;
                    LockedReturn(null);
                }
                else if (fileExists && lwt > lastFileWriteTime)
                {
                    lastFileWriteTime = lwt;
                    var changes = configurationSource.Get();

                    if (!Equals(current, changes))
                        LockedReturn(changes);
                }
            }
        }

        public void Dispose()
        {
            disposing = true;
            watcher?.Dispose();
        }
    }
}