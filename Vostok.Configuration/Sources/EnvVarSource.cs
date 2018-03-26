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
    /// Ini converter to RawSettings tree from file
    /// </summary>
    public class EnvVarSource : IConfigurationSource
    {
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly TimeSpan observePeriod;
        private string current;
        private RawSettings currentTree;
        private bool disposing;

        public EnvVarSource(TimeSpan observePeriod = default)
        {
            observers = new BehaviorSubject<RawSettings>(null);
            this.observePeriod = observePeriod.Milliseconds < 100 ? 100.Milliseconds() : observePeriod;
            disposing = false;

            ThreadRunner.Run(WatchVars);
        }

        public RawSettings Get() => Get(GetVariables());

        private RawSettings Get(string vars)
        {
            using (var source = new IniStringSource(vars))
                return source.Get();
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
                var subscribtion = observers.Where(s => s != null).SubscribeSafe(observer);
                if (currentTree != null)
                    observer.OnNext(currentTree);
                return subscribtion;
            });
        }

        public void Dispose()
        {
            disposing = true;
            observers.Dispose();
        }

        private void WatchVars()
        {
            while (!disposing)
            {
                Thread.Sleep(observePeriod);
                if (disposing) break;
                if (!observers.HasObservers) continue;

                var changes = GetVariables();
                
                if (!Equals(current, changes))
                {
                    currentTree = Get(changes);
                    observers.OnNext(currentTree);
                    current = changes;
                }
            }
        }
    }
}