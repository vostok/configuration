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
    /// <summary>
    /// Watching changes in as single text file
    /// </summary>
    internal class SingleFileWatcher : IObservable<string>
    {
        private const string DefaultSettingsValue = "\u0001";
        private readonly int watcherPeriod = (int)5.Seconds().TotalMilliseconds;

        private readonly string filePath;
        private readonly List<IObserver<string>> observers;
        private readonly FileSystemWatcher fileWatcher;
        private Task task;
        private CancellationTokenSource tokenSource;
        private string currentValue;
        private CancellationToken token;

        /// <summary>
        /// Creates a <see cref="SingleFileWatcher"/> instance with given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Full file path</param>
        public SingleFileWatcher([NotNull] string filePath)
        {
            this.filePath = filePath;
            observers = new List<IObserver<string>>();
            currentValue = DefaultSettingsValue;

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fileWatcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            if (tokenSource == null)
            {
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                task = new Task(WatchFile, token);
                task.Start();
            }
            else if (currentValue != DefaultSettingsValue)
                observer.OnNext(currentValue);

            return Disposable.Create(
                () =>
                {
                    if (observers.Contains(observer))
                        observers.Remove(observer);
                    if (observers.Count == 0)
                        StopTask();
                });
        }

        public bool HasObservers() => observers.Any();

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
                    if (CheckFile(out var changes))
                    {
                        currentValue = changes;
                        foreach (var observer in observers.ToArray())
                            observer.OnNext(currentValue);
                    }
                }
                catch (IOException)
                {
                }
                catch (Exception e)
                {
                    foreach (var observer in observers.ToArray())
                        observer.OnError(e);
                }

                if (token.IsCancellationRequested) break;
                fileWatcher.WaitForChanged(WatcherChangeTypes.All, watcherPeriod);
            }
        }

        private bool CheckFile(out string changes)
        {
            var fileExists = File.Exists(filePath);
            changes = null;

            if (!fileExists && currentValue != null)
                return true;
            else if (fileExists)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    changes = reader.ReadToEnd();

                if (currentValue != changes)
                    return true;
            }

            return false;
        }
    }
}