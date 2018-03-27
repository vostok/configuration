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
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly TimeSpan observePeriod;
        private string current;
        private RawSettings currentTree;
        private bool disposing;

        public EnvironmentVariablesSource(TimeSpan observePeriod = default)
        {
            observers = new BehaviorSubject<RawSettings>(null);
            this.observePeriod = observePeriod.Milliseconds < 100 ? 100.Milliseconds() : observePeriod; // CR(krait): milliseconds -> TimeSpan, see SettingsFileWatcher.
            disposing = false;

            // CR(krait): No, this source also cannot create threads for every instance. It should work same as file sources.
            ThreadRunner.Run(WatchVars);
        }

        public RawSettings Get() => Get(GetVariables());

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                var subscription = observers.Where(s => s != null).SubscribeSafe(observer);
                if (currentTree != null)
                    observer.OnNext(currentTree);
                return subscription;
            });
        }

        public void Dispose()
        {
            disposing = true;
            observers.Dispose();
        }

        private static RawSettings Get(string vars)
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