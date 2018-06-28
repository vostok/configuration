using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Synchronization;
using Vostok.Commons.Extensions;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <inheritdoc />
    /// <summary>
    /// Watching changes in as single text file
    /// </summary>
    internal class SingleFileWatcher : IObservable<string>
    {
        private readonly string filePath;
        private readonly FileSourceSettings settings;

        private readonly Subject<string> observers;
        private readonly AtomicBoolean taskIsRun;

        private volatile ValueWrapper currentValueWrapper;
        private CancellationTokenSource tokenDelaySource;
        private CancellationToken tokenDelay;

        /// <summary>
        /// Creates a <see cref="SingleFileWatcher"/> instance with given parameter <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <param name="fileSourceSettings"></param>
        public SingleFileWatcher([NotNull] string filePath, FileSourceSettings fileSourceSettings)
        {
            this.filePath = filePath;
            settings = fileSourceSettings ?? new FileSourceSettings();
            observers = new Subject<string>();
            taskIsRun = new AtomicBoolean(false);

            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory;
            var fileWatcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            fileWatcher.Changed += OnFileWatcherEvent;
            fileWatcher.EnableRaisingEvents = true;
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            observers.Subscribe(observer);
            if (currentValueWrapper != null)
                observer.OnNext(currentValueWrapper.Value);

            if (taskIsRun.TrySetTrue())
                Task.Run(WatchFile);

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
                    if (CheckFile(out var changes))
                    {
                        currentValueWrapper = new ValueWrapper(changes);
                        observers.OnNext(changes);
                    }
                }
                catch (Exception e)
                {
                    observers.OnError(e);
                }

                if (tokenDelaySource == null || tokenDelay.IsCancellationRequested)
                {
                    tokenDelaySource = new CancellationTokenSource();
                    tokenDelay = tokenDelaySource.Token;
                }

                await Task.Delay(settings.FileWatcherPeriod, tokenDelay).SilentlyContinue();
            }
        }

        private bool CheckFile(out string changes)
        {
            changes = null;

            if (File.Exists(filePath))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(fileStream, settings.Encoding))
                    changes = reader.ReadToEnd();
            }

            return currentValueWrapper == null ||  currentValueWrapper.Value != changes;
        }

        private class ValueWrapper
        {
            public ValueWrapper(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}