/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// A source that combines settings from several other sources, resolving possible conflicts in favor of sources which one came earlier in the list
    /// </summary>
    public class CombinedSource : IConfigurationSource
    {
        [NotNull]
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SourceCombineOptions sourceCombineOptions;
        private readonly CombineOptions combineOptions;
        private readonly IList<IObserver<ISettingsNode>> observers;
//        private readonly BehaviorSubject<ISettingsNode> observers;
        private readonly ConcurrentBag<IDisposable> watchers;
//        private readonly ConcurrentBag<IDisposable> watchers;
//        private readonly IList<IDisposable> watchers;
        private readonly IDictionary<IConfigurationSource, ISettingsNode> sourcesSettings;
        private ISettingsNode currentValue;
        private readonly TaskSource taskSource;
        private readonly object locker;
        private bool neverMerged = true;

        /// <summary>
        /// <para>Creates a <see cref="CombinedSource"/> instance new source using combining options.</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configuration sources to combine</param>
        /// <param name="sourceCombineOptions">Options for source combining</param>
        /// <param name="combineOptions">Options for lists combining</param>
        public CombinedSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources,
            SourceCombineOptions sourceCombineOptions,
            CombineOptions combineOptions)
        {
            this.sources = sources;
            this.sourceCombineOptions = sourceCombineOptions;
            this.combineOptions = combineOptions;
            locker = new object();
//            sourcesSettings = new Dictionary<IConfigurationSource, ISettingsNode>();
//            observers = new BehaviorSubject<ISettingsNode>(currentSettings);
//            watchers = new List<IDisposable>(sources.Count);
            taskSource = new TaskSource();
            watchers = new ConcurrentBag<IDisposable>();
            observers = new List<IObserver<ISettingsNode>>();
            sourcesSettings = new Dictionary<IConfigurationSource, ISettingsNode>();
            /*foreach (var source in sources)
            {
                var src = source;
                var watcher = source.Observe().Subscribe(
                    settings =>
                    {
                        sourcesSettings[src] = settings;
                        MergeIntoCurrentSettings(sourcesSettings.Values.ToArray());
                        observers.OnNext(currentSettings);
                    });
                watchers.Add(watcher);
            }

            sourcesSettings = sources.ToDictionary(s => s, s => s.Get());
            MergeIntoCurrentSettings(sourcesSettings.Values.ToArray());#1#
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="CombinedSource" /> instance new source using default combining options</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configurations</param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources.ToArray(), SourceCombineOptions.LastIsMain, CombineOptions.Override)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously combined configurations. Null if sources where not specified.
        /// </summary>
        /// <returns>Combine as RawSettings tree</returns>
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="ISettingsNode"/> changes in any of sources.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<ISettingsNode> Observe() =>
            Observable.Create<ISettingsNode>(
                observer =>
                {
                    if (!watchers.Any())
                    {
                        foreach (var source in sources)
                        {
                            var src = source;
                            sourcesSettings.Add(src, src.Get());
                            var watcher = source.Observe()
                                .Subscribe(
                                    newSettings =>
                                    {
                                        lock (locker)
                                        {
                                            if (!Equals(newSettings, sourcesSettings[src]))
                                            {
                                                sourcesSettings[src] = newSettings;
                                                currentValue = Merge(sourcesSettings.Values);
                                                observer.OnNext(currentValue);
                                            }
                                            if (neverMerged && currentValue != null)
                                                observer.OnNext(currentValue);
                                        }
                                    });
                            watchers.Add(watcher);
                        }

                        currentValue = Merge(sourcesSettings.Values.Cast<ISettingsNode>());
                    }

                    if (!observers.Contains(observer))
                        observers.Add(observer);
                    observer.OnNext(currentValue);

                    return Disposable.Create(
                        () =>
                        {
                            if (observers.Contains(observer))
                                observers.Remove(observer);
                        });
                });

        public void Dispose()
        {
            foreach (var watcher in watchers)
                watcher.Dispose();
        }

        private ISettingsNode Merge(IEnumerable<ISettingsNode> settingses, string name = "root")
        {
            neverMerged = false;

            var sets = settingses as ISettingsNode[] ?? settingses.ToArray();
            if (!sets.Any() || sets.Any(s => s == null)) return null;

            var datas = sets.Select(s => s.Children.ToArray()).ToArray();
            var lookup = datas.SelectMany(d => d).ToLookup(d => d.Name);

            SortedDictionary<string, ISettingsNode> dict = null;
            if (combineOptions == CombineOptions.Override)
                dict = lookup.ToSortedDictionary(l => l.Key,
                    l => sourceCombineOptions == SourceCombineOptions.FirstIsMain ? l.First() : l.Last(), new ChildrenKeysComparer());
            else if (combineOptions == CombineOptions.DeepMerge)
                dict = lookup.ToSortedDictionary(l => l.Key,
                    l => l.All(s => !s.Children.Any())
                        ? (sourceCombineOptions == SourceCombineOptions.FirstIsMain ? l.First() : l.Last())
                        : Merge(l, l.Key),
                    new ChildrenKeysComparer());

            return new ObjectNode(dict, name);
        }
    }
}*/