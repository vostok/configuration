using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources.Watchers
{

    /// <inheritdoc />
    /// <summary>
    /// Watching changes in as single text file
    /// </summary>
    internal class SingleFileWatcher : IObservable<string>
    {
        private readonly int watcherPeriod = (int)5.Seconds().TotalMilliseconds; // todo(Mansiper): choose value

        private readonly string filePath;
        private readonly Encoding encoding;

        // CR(krait): Why couldn't you just use a Subject<string> and avoid locks and half the code? Btw, if you peek inside Subject<T>, you'll see a correct way to keep a list of subscribers without locks.
        // CR(krait): When switching to Subject<T>, please keep in mind to correctly push the current value to new subscribers.
        private readonly List<IObserver<string>> observers;
        private readonly FileSystemWatcher fileWatcher;
        private readonly object locker;
        private Task task;
        private CancellationTokenSource tokenSource;
        private string currentValue;
        private bool initialized;
        private CancellationToken token;

        /// <summary>
        /// Creates a <see cref="SingleFileWatcher"/> instance with given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <param name="encoding">File encoding</param>
        public SingleFileWatcher([NotNull] string filePath, Encoding encoding)
        {
            this.filePath = filePath;
            this.encoding = encoding;
            observers = new List<IObserver<string>>();
            currentValue = null;
            initialized = false;

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fileWatcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            locker = new object();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            lock (locker)
                if (tokenSource == null)
                {
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    task = new Task(WatchFile, token);
                    task.Start();
                }
                else if (initialized)
                    observer.OnNext(currentValue);

            return Disposable.Create(
                () =>
                {
                    lock (locker)
                    {
                        if (observers.Contains(observer))
                            observers.Remove(observer);
                        if (observers.Count == 0)
                            StopTask();
                    }
                });
        }

        private void StopTask()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
        }

        private void WatchFile()
        {
            while (true)
            {
                if (token.IsCancellationRequested) break;

                try
                {
                    lock (locker)
                        if (CheckFile(out var changes))
                        {
                            initialized = true;
                            currentValue = changes;
                            foreach (var observer in observers)
                                observer.OnNext(currentValue);
                        }
                }
                catch (IOException)
                {
                }
                catch (Exception e)
                {
                    lock (locker)
                        foreach (var observer in observers)
                            observer.OnError(e);
                }

                if (token.IsCancellationRequested) break;
                fileWatcher.WaitForChanged(WatcherChangeTypes.All, watcherPeriod); // CR(krait): Now you just spend a thread pool thread instead of you own. The idea was to sleep without consuming a thread.
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