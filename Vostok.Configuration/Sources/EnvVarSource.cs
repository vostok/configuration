using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to RawSettings tree from file
    /// </summary>
    public class EnvVarSource : IConfigurationSource
    {
        private readonly List<IObserver<RawSettings>> observers;
        private readonly object sync;
        private readonly TimeSpan observePeriod;
        private string current;
        private RawSettings currentTree;
        private bool disposing;

        public EnvVarSource(TimeSpan observePeriod = default)
        {
            observers = new List<IObserver<RawSettings>>();
            sync = new object();
            this.observePeriod = observePeriod.Milliseconds < 100 ? TimeSpan.FromMilliseconds(100) : observePeriod;
            disposing = false;

            ThreadRunner.Run(WatchVars);
        }

        public RawSettings Get() => Get(GetVariables());

        private RawSettings Get(string vars)
        {
            using (var iss = new IniStringSource(vars))
                return iss.Get();
        }

        private string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(observer);
                    if (currentTree != null)
                        observer.OnNext(currentTree);
                }
                return Disposable.Create(() =>
                {
                    lock (sync)
                    {
                        observers.Remove(observer);
                    }
                });
            });
        }

        public void Dispose()
        {
            disposing = true;
        }

        private void WatchVars()
        {
            void LockedReturn(string changes)
            {
                lock (sync)
                {
                    currentTree = Get(changes);
                    foreach (var observer in observers)
                        observer.OnNext(currentTree);
                    current = changes;
                }
            }

            while (!disposing)
            {
                Thread.Sleep(observePeriod);
                if (disposing) break;
                if (observers.Count == 0) continue;

                var changes = GetVariables();
                
                if (!Equals(current, changes))
                    LockedReturn(changes);
            }
        }
    }
}