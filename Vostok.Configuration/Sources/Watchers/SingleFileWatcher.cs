using System;
using System.IO;
using System.Reactive.Subjects;
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
        private const int True = 1;
        private const int False = 0;
        private readonly TimeSpan watcherPeriod = 5.Seconds(); // todo(Mansiper): choose value

        private readonly string filePath;
        private readonly Encoding encoding;

        private readonly Subject<string> observers;
        private readonly object locker;
        private readonly AtomicBoolean initialized;
        private CancellationTokenSource tokenDelaySource;
        private string currentValue;
        private int firstSubscription;
        private CancellationToken tokenDelay;

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
            firstSubscription = True;

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory;
            var fileWatcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            fileWatcher.Changed += OnFileWatcherEvent;
            fileWatcher.EnableRaisingEvents = true;
            locker = new object();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            observers.Subscribe(observer);
            if (initialized)
                observer.OnNext(currentValue);

            if (Interlocked.Exchange(ref firstSubscription, False) == True)
            {
                tokenDelaySource = new CancellationTokenSource();
                tokenDelay = tokenDelaySource.Token;
                Task.Run(WatchFile);
            }

            return observers;
        }

        private void OnFileWatcherEvent(object sender, FileSystemEventArgs args)
        {
            if (tokenDelaySource != null && !tokenDelaySource.IsCancellationRequested)
                tokenDelaySource.Cancel();
        }

        private async Task WatchFile()
        {
            while (true)
            {
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

                if (tokenDelay.IsCancellationRequested)
                {
                    tokenDelaySource = new CancellationTokenSource();
                    tokenDelay = tokenDelaySource.Token;
                }

                await Task.Delay(watcherPeriod, tokenDelay).ContinueWith(_ => {});
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