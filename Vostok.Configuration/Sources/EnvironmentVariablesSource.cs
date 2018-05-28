using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="IRawSettings"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly TimeSpan minObservationPeriod = 100.Milliseconds();
        private readonly TimeSpan defaultObservationPeriod = 1.Minutes();
        private readonly IList<IObserver<IRawSettings>> observers;
        private readonly TimeSpan observationPeriod;
        private readonly object locker;
        private readonly TaskSource taskSource;
        private IRawSettings currentValue;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private Task task;

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

            locker = new object();
            lock (locker)
            {
                observers = new List<IObserver<IRawSettings>>();
                taskSource = new TaskSource();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="IRawSettings"/> tree.
        /// </summary>
        public IRawSettings Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="IRawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe() =>
            Observable.Create<IRawSettings>(
                observer =>
                {
                    lock (locker)
                    {
                        if (!observers.Contains(observer))
                            observers.Add(observer);
                        if (tokenSource == null)
                        {
                            tokenSource = new CancellationTokenSource();
                            token = tokenSource.Token;
                            task = new Task(WatchVariables, token);
                            task.Start();
                        }
                        else
                            observer.OnNext(currentValue);
                    }

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
                });

        public void Dispose()
        {
            StopTask();
        }

        private void StopTask()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
        }

        private static IRawSettings GetSettings(string vars)
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

            while (true)
            {
                if (token.IsCancellationRequested) break;

                if (nextCheck <= DateTime.UtcNow)
                    try
                    {
                        currentValue = GetSettings(GetVariables());
                        nextCheck = DateTime.UtcNow + observationPeriod;
                        foreach (var observer in observers.ToArray())
                            observer.OnNext(currentValue);
                    }
                    catch (Exception e)
                    {
                        foreach (var observer in observers.ToArray())
                            observer.OnError(e);
                    }

                if (token.IsCancellationRequested) break;
                Thread.Sleep(minObservationPeriod);
            }
        }
    }
}