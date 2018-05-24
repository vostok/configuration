using System;
using System.Reactive.Linq;
using System.Threading;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for converters to <see cref="IRawSettings"/> tree from file
    /// </summary>
    public class BaseFileSource : IConfigurationSource
    {
        //        private readonly BehaviorSubject<RawSettings> observers;
        //        private readonly IDisposable fileWatcherSubscribtion;
        private readonly IObservable<IRawSettings> fileWatcher;
        private IRawSettings current;
        private AutoResetEvent msg;

        /// <summary>
        /// <para>Creates a <see cref="BaseFileSource"/> instance.</para>
        /// <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        protected BaseFileSource(string filePath, Func<string, IRawSettings> parseSettings)
        {
            //            observers = new BehaviorSubject<RawSettings>(current);

            msg = new AutoResetEvent(false);
            fileWatcher = new SingleFileWatcher(filePath).Select(
                str =>
                {
                    msg?.Set();
                    msg = null;

                    current = parseSettings(str);
                    return current;
                });
            /*fileWatcher = SettingsFileWatcher.WatchFile(filePath, this).Select(
                str =>
                {
                    msg.Set();
                    msg = null;

                    current = parseSettings(str);
                    return current;
                });*/
            /*var msg = new AutoResetEvent(false);
            fileWatcherSubscribtion = fileWatcher.Subscribe(
                str =>
                {
                    current = parseSettings(str);
                    observers.OnNext(current);
                    msg.Set();
                });
            msg.WaitOne();*/
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="IRawSettings"/> tree.
        /// </summary>
        public IRawSettings Get()
        {
            msg?.WaitOne();
            return current;
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="IRawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe() => fileWatcher;
//            Observable.Create<RawSettings>(observer =>
//                observers.Select(settings => current).Subscribe(observer));

        public void Dispose()
        {
            //            observers.Dispose();
            //            fileWatcherSubscribtion?.Dispose();
        }
    }
}