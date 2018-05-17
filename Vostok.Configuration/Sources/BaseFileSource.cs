using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for converters to <see cref="RawSettings"/> tree from file
    /// </summary>
    public class BaseFileSource : IConfigurationSource
    {
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly IDisposable fileWatcherSubscribtion;
        private RawSettings current;

        /// <summary>
        /// <para>Creates a <see cref="BaseFileSource"/> instance.</para>
        /// <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        /// <param name="onError">Callback on error</param>
        protected BaseFileSource(string filePath, Func<string, RawSettings> parseSettings, TimeSpan observationPeriod = default, Action<Exception> onError = null)
        {
            observers = new BehaviorSubject<RawSettings>(current);

            var fileWatcher = SettingsFileWatcher.WatchFile(filePath, this, observationPeriod, onError);
            var msg = new AutoResetEvent(false);
            fileWatcherSubscribtion = fileWatcher.Subscribe(
                str =>
                {
                    current = parseSettings(str);
                    observers.OnNext(current);
                    msg.Set();
                });
            msg.WaitOne();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="RawSettings"/> tree.
        /// </summary>
        public RawSettings Get() => current;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="RawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<RawSettings> Observe() =>
            Observable.Create<RawSettings>(observer =>
                observers.Select(settings => current).Subscribe(observer));

        public void Dispose()
        {
            observers.Dispose();
            fileWatcherSubscribtion?.Dispose();
        }
    }
}