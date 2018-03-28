using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Kontur.ClusterConfig.Client;
using Vostok.Commons;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.ClusterConfig
{
    public interface IClusterConfigClientProxy
    {
        Dictionary<string, List<string>> GetAll();
        List<string> GetByKey(string key);
        Dictionary<string, List<string>> GetByPrefix(string prefix);
    }

    public class ClusterConfigClientProxy : IClusterConfigClientProxy
    {
        public Dictionary<string, List<string>> GetAll() => ClusterConfigClient.GetAll();

        public List<string> GetByKey(string key) => ClusterConfigClient.GetByKey(key);

        public Dictionary<string, List<string>> GetByPrefix(string prefix) => ClusterConfigClient.GetByPrefix(prefix);
    }

    public class ClusterConfigSource : IConfigurationSource
    {
        private static readonly TimeSpan MinObservationPeriod = 1.Minutes();

        private readonly string prefix;
        private readonly string key;
        private readonly TimeSpan observePeriod;

        public ClusterConfigSource(string prefix, string key, TimeSpan observePeriod = default) //todo: callback?
        {
            this.prefix = prefix;
            this.key = key;
            this.observePeriod = observePeriod < MinObservationPeriod ? MinObservationPeriod : observePeriod;
            FixedPeriodSettingsWatcher.StartFixedPeriodSettingsWatcher(1.Seconds(), 1.Minutes(), GetAll, this.observePeriod);
        }

        public ClusterConfigSource(TimeSpan observePeriod = default)
            : this(null, null, observePeriod)
        { }

        public RawSettings Get()
        {
            var emptyPrefix = string.IsNullOrWhiteSpace(prefix);
            var emptyKey = string.IsNullOrWhiteSpace(key);

            if (emptyPrefix && emptyKey)
                return ParseCcTree(new ClusterConfigClientProxy().GetAll());
            else if (!emptyPrefix && !emptyKey)
                return ParseCcList(new ClusterConfigClientProxy().GetByKey($"{prefix}/{key}"));
            else if (!emptyPrefix)
                return ParseCcTree(new ClusterConfigClientProxy().GetByPrefix(prefix));
            else
                return ParseCcTree(new ClusterConfigClientProxy().GetAll(), true);
        }

        private RawSettings GetAll() => ParseCcTree(new ClusterConfigClientProxy().GetAll());

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
            var prefScope = prefix?.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var keyScope = key?.ToEnumerable();
            var scope = prefScope ?? keyScope;
            if (prefScope != null && keyScope != null)
                scope = scope.Concat(keyScope);
            return FixedPeriodSettingsWatcher.Observe(observePeriod)
                .Select(s => new ScopedSource(s, scope.ToArray()).Get())
                .Where(s => !Equals(s, Get()));
        }

        public void Dispose()
        {
            FixedPeriodSettingsWatcher.RemoveObservers(observePeriod);
        }
    }
}