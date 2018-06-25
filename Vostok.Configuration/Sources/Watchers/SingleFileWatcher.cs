using System;
using System.IO;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Conversions;
using Vostok.Commons.Synchronization;

namespace Vostok.Configuration.Sources.Watchers
{

    /// <inheritdoc />
    /// <summary>
    /// Watching changes in as single text file
    /// </summary>
    internal class SingleFileWatcher : IObservable<string>
    {
        // CR(krait): Use TimeSpan.
        private readonly int watcherPeriod = (int)5.Seconds().TotalMilliseconds; // todo(Mansiper): choose value

        private readonly string filePath;
        private readonly Encoding encoding;

        private readonly Subject<string> observers;
        private readonly object locker;
        // CR(krait): Who cancels tokenTaskSource?
        private CancellationTokenSource tokenTaskSource, tokenDelaySource;
        private string currentValue;
        private readonly AtomicBoolean initialized;
        private CancellationToken tokenTask, tokenDelay;

        /// <summary>
        /// Creates a <see cref="SingleFileWatcher"/> instance with given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <param name="encoding">File encoding</param>
        public SingleFileWatcher([NotNull] string filePath, Encoding encoding)
        {
            this.filePath = filePath;
            this.encoding = encoding;
            observers = new Subject<string>();
            currentValue = null;
            initialized = new AtomicBoolean(false);

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // CR(krait): Seems like path can still be null.
            var fileWatcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            fileWatcher.Changed += OnFileWatcherEvent;
            // CR(krait): Doesn't 'Changed' cover all other events?
            fileWatcher.Created += OnFileWatcherEvent;
            fileWatcher.Deleted += OnFileWatcherEvent;
            fileWatcher.Renamed += OnFileWatcherEvent;
            fileWatcher.EnableRaisingEvents = true;
            locker = new object();
        }

        private void OnFileWatcherEvent(object sender, FileSystemEventArgs args)
        {
            if (tokenDelaySource != null && !tokenDelaySource.IsCancellationRequested)
                tokenDelaySource.Cancel();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            observers.Subscribe(observer);
            if (initialized)
                observer.OnNext(currentValue);

            // CR(krait): Please get rid of locks in this class, they have no just reason to exist. For example, you could try to wrap all state in one object and atomically substitute it via Interlocked.
            lock (locker)
                if (tokenTaskSource == null)
                {
                    tokenTaskSource = new CancellationTokenSource();
                    tokenDelaySource = new CancellationTokenSource();
                    tokenTask = tokenTaskSource.Token;
                    tokenDelay = tokenDelaySource.Token;
                    Task.Run(WatchFile, tokenTask);
                }

            return observers;
        }

        // CR(krait): What's this?
        /*private void StopTask()
        {
            if (tokenTaskSource != null && !tokenTaskSource.IsCancellationRequested)
                tokenTaskSource.Cancel();
        }*/

        private async Task WatchFile()
        {
            while (true)
            {
                if (tokenTask.IsCancellationRequested) break;
                
                try
                {
                    lock (locker)
                        if (CheckFile(out var changes))
                        {
                            initialized.TrySetTrue();
                            currentValue = changes;
                            observers.OnNext(currentValue);
                        }
                }
                catch (IOException)
                {
                }
                catch (Exception e)
                {
                    observers.OnError(e);
                }

                if (tokenTask.IsCancellationRequested) break;

                if (tokenDelay.IsCancellationRequested)
                {
                    tokenDelaySource = new CancellationTokenSource();
                    tokenDelay = tokenDelaySource.Token;
                }

                // CR(krait): There is a more elegant way to achieve this. Instead of try-catch, just add an empty continuation: .ContinueWith(_ => { });
                // CR(krait): Btw, it's a useful pattern worth moving into vostok.commons. See https://github.com/vostok/airlock.client/blob/temp/Vostok.Airlock.Client/TaskExtensions.cs
                try
                {
                    await Task.Delay(watcherPeriod, tokenDelay);
                }
                catch (TaskCanceledException) { }
            }
        }

        private bool CheckFile(out string changes)
        {
            var fileExists = File.Exists(filePath);
            changes = null;

            if (!fileExists && (currentValue != null || !initialized))
                return true;
            else if (fileExists)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(fileStream, encoding))
                    changes = reader.ReadToEnd();

                if (currentValue != changes)
                    return true;
            }

            return false;
        }
    }
}