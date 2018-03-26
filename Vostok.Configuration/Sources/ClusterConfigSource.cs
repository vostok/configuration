using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Kontur.ClusterConfig.Client;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    public class ClusterConfigSource: IConfigurationSource
    {
        private readonly string prefix;
        private readonly string key;
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly TimeSpan observePeriod;
        private RawSettings current;
        private bool disposing;

        public ClusterConfigSource(string prefix, string key, TimeSpan observePeriod = default)
        {
            this.prefix = prefix;
            this.key = key;
            this.observePeriod = observePeriod.Milliseconds < 1.Minutes().Milliseconds ? 1.Minutes() : observePeriod;
            observers = new BehaviorSubject<RawSettings>(null);
            disposing = false;

            ThreadRunner.Run(WatchSettings);
        }

        public ClusterConfigSource(TimeSpan observePeriod = default)
            : this(null, null, observePeriod)
        { }

        public RawSettings Get()
        {
            var emptyPrefix = string.IsNullOrWhiteSpace(prefix);
            var emptyKey = string.IsNullOrWhiteSpace(key);

            if (emptyPrefix && emptyKey)
                return ParseCcTree(ClusterConfigClient.GetAll());
            else if (!emptyPrefix && !emptyKey)
                return ParseCcList(ClusterConfigClient.GetByKey($"{prefix}/{key}"));
            else if (!emptyPrefix)
                return ParseCcTree(ClusterConfigClient.GetByPrefix(prefix));
            else
                return ParseCcTree(ClusterConfigClient.GetAll(), true);
        }

        private RawSettings ParseCcTree(Dictionary<string, List<string>> tree, bool byKey = false)
        {
            if (!byKey)
                return new RawSettings(tree.ToDictionary(pair => pair.Key, pair => ParseCcList(pair.Value)));
            if (tree.ContainsKey(key))
                return ParseCcList(tree[key]);

            throw new ArgumentException($"Key \"{key}\" does not exist.");
        }

        private RawSettings ParseCcList(List<string> tree) => 
            new RawSettings(tree.Select(v => new RawSettings(v)).ToList());

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                var subscribtion = observers.Where(s => s != null).SubscribeSafe(observer);
                if (current != null)
                    observer.OnNext(current);
                return subscribtion;
            });
        }
        public void Dispose()
        {
            disposing = true;
            observers.Dispose();
        }

        private void WatchSettings()
        {
            while (!disposing)
            {
                Thread.Sleep(observePeriod);
                if (disposing) break;
                if (!observers.HasObservers) continue;

                var changes = Get();

                if (!Equals(current, changes))
                {
                    observers.OnNext(current);
                    current = changes;
                }
            }
        }
    }
}