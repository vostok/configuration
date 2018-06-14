using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Comparers;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.SettingsTree;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.ClusterConfig
{
    /// <inheritdoc />
    /// <summary>
    /// Cluster config converter to <see cref="ISettingsNode"/> tree
    /// </summary>
    public class ClusterConfigSource : IConfigurationSource
    {
        private readonly TimeSpan minObservationPeriod = 1.Minutes();
        private readonly TimeSpan checkPeriod = 100.Milliseconds();
        private readonly IList<IObserver<ISettingsNode>> observers;
        private readonly TimeSpan observationPeriod;
        private readonly string prefix;
        private readonly string key;
        private readonly IClusterConfigClientProxy clusterConfigClient;
        private readonly object locker;
        private readonly TaskSource taskSource;
        private ISettingsNode currentValue;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private Task task;

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

            locker = new object();
            observers = new List<IObserver<ISettingsNode>>();
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Returns last parsed configurations</para>
        /// <para>Waits for first read.</para>
        /// </summary>
        /// <exception cref="Exception">Only on first read. Otherwise returns last parsed value.</exception>
        /// <returns>Combine as RawSettings tree</returns>
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="ISettingsNode"/> changes in source.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<ISettingsNode> Observe() =>
            Observable.Create<ISettingsNode>(
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
                            task = new Task(WatchClusterConfig, token);
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
            tokenSource?.Dispose();
        }

        private void StopTask()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
        }

        private ISettingsNode ReadSettings()
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

        private ISettingsNode ParseCcTree(IReadOnlyDictionary<string, List<string>> tree, bool byKey = false)
        {
            if (!byKey)
                return new ObjectNode(tree.ToSortedDictionary(pair => pair.Key, pair => ParseCcList(pair.Value), new ChildrenKeysComparer()));
            if (tree.ContainsKey(key))
                return ParseCcList(tree[key]);

            throw new ArgumentException($"{nameof(ClusterConfigSource)}: key \"{key}\" does not exist.");
        }

        private static ISettingsNode ParseCcList(IEnumerable<string> tree) =>
            new ArrayNode(tree.Select(e => new ValueNode(e)).ToList());

        private void WatchClusterConfig()
        {
            var nextCheck = DateTime.UtcNow.AddDays(-1);

            while (true)
            {
                if (token.IsCancellationRequested) break;

                if (nextCheck <= DateTime.UtcNow)
                {
                    try
                    {
                        lock (locker)
                        {
                            var newSettings = ReadSettings();
                            if (!Equals(newSettings, currentValue))
                            {
                                currentValue = newSettings;
                                foreach (var observer in observers.ToArray())
                                    observer.OnNext(currentValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        foreach (var observer in observers.ToArray())
                            observer.OnError(e);
                    }
                    finally
                    {
                        nextCheck = DateTime.UtcNow + observationPeriod;
                    }
                }

                if (token.IsCancellationRequested) break;
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