using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.ClusterConfig
{
    /// <inheritdoc />
    /// <summary>
    /// Cluster config converter to <see cref="IRawSettings"/> tree
    /// </summary>
    public class ClusterConfigSource : IConfigurationSource
    {
        private readonly TimeSpan minObservationPeriod = 1.Minutes();
        private readonly TimeSpan checkPeriod = 100.Milliseconds();
        private readonly BehaviorSubject<IRawSettings> observers;
        private readonly TimeSpan observationPeriod;
        private readonly AutoResetEvent firstRead;
        private readonly string prefix;
        private readonly string key;
        private readonly IClusterConfigClientProxy clusterConfigClient;
        private bool needStop;
        private IRawSettings currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="ClusterConfigSource"/> instance using given parameters <paramref name="prefix"/>, <paramref name="key"/>, and <paramref name="observationPeriod"/>.</para>
        /// <para>Current config source uses <paramref name="prefix"/> and <paramref name="key"/> to get subtree from cluster config</para>
        /// </summary>
        /// <param name="prefix">Prefix for cluster config search in.</param>
        /// <param name="key">Key for cluster config search in.</param>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 60000)</param>
        public ClusterConfigSource(string prefix, string key, TimeSpan observationPeriod = default)
            : this(prefix, key, new ClusterConfigClientProxy(), observationPeriod)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="ClusterConfigSource"/> instance using given parameter <paramref name="observationPeriod"/>.</para>
        /// <para>Current config source always return all settings from cluster config</para>
        /// </summary>
        /// <param name="observationPeriod">Observe period in ms (min 100, default 60000)</param>
        public ClusterConfigSource(TimeSpan observationPeriod = default)
            : this(null, null, new ClusterConfigClientProxy(), observationPeriod)
        {
        }

        internal ClusterConfigSource(string prefix, string key, IClusterConfigClientProxy clusterConfigClient, TimeSpan observationPeriod = default, bool forTest = false)
        {
            this.prefix = prefix;
            this.key = key;
            this.clusterConfigClient = clusterConfigClient;
            if (!forTest)
                this.observationPeriod = observationPeriod < minObservationPeriod ? minObservationPeriod : observationPeriod;
            else
                this.observationPeriod = observationPeriod;

            needStop = false;
            observers = new BehaviorSubject<IRawSettings>(currentSettings);

            firstRead = new AutoResetEvent(false);
            ThreadRunner.Run(WatchClusterConfig);
            if (!firstRead.WaitOne(1.Seconds(), false))
                throw new TimeoutException();
            firstRead = null;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed configurations. Null if sources where not specified.
        /// </summary>
        /// <returns>Combine as RawSettings tree</returns>
        public IRawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="IRawSettings"/> changes in source.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<IRawSettings> Observe() =>
            Observable.Create<IRawSettings>(
                observer => observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            needStop = true;
            observers.Dispose();
        }

        private RawSettings ReadSettings()
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

        private RawSettings ParseCcTree(IReadOnlyDictionary<string, List<string>> tree, bool byKey = false)
        {
            if (!byKey)
                return new RawSettings(tree.ToOrderedDictionary(pair => pair.Key, pair => ParseCcList(pair.Value)));
            if (tree.ContainsKey(key))
                return ParseCcList(tree[key]);

            throw new ArgumentException($"Key \"{key}\" does not exist.");
        }

        private static RawSettings ParseCcList(IEnumerable<string> tree) =>
            new RawSettings(tree.ToOrderedDictionary(v => v, v => new RawSettings(v)));

        private void WatchClusterConfig()
        {
            var nextCheck = DateTime.UtcNow.AddMinutes(-1);

            while (!needStop)
            {
                if (nextCheck <= DateTime.UtcNow)
                {
                    try
                    {
                        var newSettings = ReadSettings();
                        if (!Equals(newSettings, currentSettings))
                        {
                            currentSettings = newSettings;
                            observers.OnNext(currentSettings);
                        }
                    }
                    catch (Exception e)
                    {
                        firstRead?.Set();
                        Thread.CurrentThread.Abort(e);
                    }
                    finally
                    {
                        nextCheck = DateTime.UtcNow + observationPeriod;
                        firstRead?.Set();
                    }
                }

                Thread.Sleep(checkPeriod);
            }
        }

        private class ClusterConfigClientProxy : IClusterConfigClientProxy
        {
            public Dictionary<string, List<string>> GetAll() => Kontur.ClusterConfig.Client.ClusterConfigClient.GetAll();
            public List<string> GetByKey(string key) => Kontur.ClusterConfig.Client.ClusterConfigClient.GetByKey(key);
            public Dictionary<string, List<string>> GetByPrefix(string prefix) => Kontur.ClusterConfig.Client.ClusterConfigClient.GetByPrefix(prefix);
        }
    }
}