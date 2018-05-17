using System;
using System.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="RawSettings"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly TimeSpan minObservationPeriod = 100.Milliseconds();
        private readonly TimeSpan defaultObservationPeriod = 1.Minutes();
        private bool needStop;
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly TimeSpan observationPeriod;
        private readonly AutoResetEvent firstRead;
        private RawSettings currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="EnvironmentVariablesSource"/> instance using given parameter <paramref name="observationPeriod"/>.</para>
        /// </summary>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 10000)</param>
        public EnvironmentVariablesSource(TimeSpan observationPeriod = default)
        {
            this.observationPeriod = observationPeriod == default
                ? defaultObservationPeriod
                : (observationPeriod < minObservationPeriod
                    ? minObservationPeriod
                    : observationPeriod);
            needStop = false;
            observers = new BehaviorSubject<RawSettings>(currentSettings);

            firstRead = new AutoResetEvent(false);
            ThreadRunner.Run(WatchVariables);
            firstRead.WaitOne();
            firstRead = null;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="RawSettings"/> tree.
        /// </summary>
        public RawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="RawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<RawSettings> Observe() =>
            Observable.Create<RawSettings>(
                observer => observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            needStop = true;
            observers.Dispose();
        }

        private static RawSettings GetSettings(string vars)
        {
            using (var source = new IniStringSource(vars))
                return source.Get();
        }

        private static string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }

        private void WatchVariables()
        {
            var nextCheck = DateTime.UtcNow.AddMinutes(-1);

            while (!needStop)
            {
                if (nextCheck <= DateTime.UtcNow)
                {
                    currentSettings = GetSettings(GetVariables());
                    nextCheck = DateTime.UtcNow + observationPeriod;
                    observers.OnNext(currentSettings);
                    firstRead?.Set();
                }

                Thread.Sleep(minObservationPeriod);
            }
        }
    }
}