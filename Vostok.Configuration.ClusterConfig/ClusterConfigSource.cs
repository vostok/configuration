using System;
using System.Collections.Generic;
using System.Linq;
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

    public class ClusterConfigSource : IConfigurationSource
    {
        private static readonly TimeSpan MinObservationPeriod = 1.Minutes();

        private readonly string prefix;
        private readonly string key;
        private readonly TimeSpan observePeriod;
        private readonly IClusterConfigClientProxy clusterConfigClient;

        public ClusterConfigSource(string prefix, string key, TimeSpan observePeriod = default)
            : this(prefix, key, new ClusterConfigClientProxy(), observePeriod)
        { }

        public ClusterConfigSource(TimeSpan observePeriod = default)
            : this(null, null, new ClusterConfigClientProxy(), observePeriod)
        { }

        internal ClusterConfigSource(string prefix, string key, IClusterConfigClientProxy clusterConfigClient, TimeSpan observePeriod = default, bool forTest = false)
        {
            this.prefix = prefix;
            this.key = key;
            this.clusterConfigClient = clusterConfigClient;
            if (!forTest)
                this.observePeriod = observePeriod < MinObservationPeriod ? MinObservationPeriod : observePeriod;
            else
                this.observePeriod = observePeriod;
            FixedPeriodSettingsWatcher.StartFixedPeriodSettingsWatcher(forTest ? 100.Milliseconds() : 1.Seconds(), forTest ? 100.Milliseconds() : 1.Minutes(), GetAll, this.observePeriod);
        }

        public RawSettings Get()
        {
            var emptyPrefix = string.IsNullOrWhiteSpace(prefix);
            var emptyKey = string.IsNullOrWhiteSpace(key);

            if (emptyPrefix && emptyKey)
                return ParseCcTree(clusterConfigClient.GetAll());
            else if (!emptyPrefix && !emptyKey)
                return ParseCcList(clusterConfigClient.GetByKey($"{prefix}/{key}"));
            else if (!emptyPrefix)
                return ParseCcTree(clusterConfigClient.GetByPrefix(prefix));
            else
                return ParseCcTree(clusterConfigClient.GetAll(), true);
        }

        private RawSettings GetAll() => ParseCcTree(clusterConfigClient.GetAll());

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
                /*.Select(s => new ScopedSource(s, scope.ToArray()).Get())
                .Where(s => !Equals(s, Get()))*/;
        }

        public void Dispose()
        {
            FixedPeriodSettingsWatcher.RemoveObservers(observePeriod);
        }

        private class ClusterConfigClientProxy : IClusterConfigClientProxy
        {
            public Dictionary<string, List<string>> GetAll() => Kontur.ClusterConfig.Client.ClusterConfigClient.GetAll();

            public List<string> GetByKey(string key) => Kontur.ClusterConfig.Client.ClusterConfigClient.GetByKey(key);

            public Dictionary<string, List<string>> GetByPrefix(string prefix) => Kontur.ClusterConfig.Client.ClusterConfigClient.GetByPrefix(prefix);
        }
    }
}