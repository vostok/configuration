using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// A source that combines settings from several other sources, resolving possible conflicts in favor of sources which one came earlier in the list
    /// </summary>
    public class CombinedSource : IConfigurationSource, IDisposable // CR(krait): Are you sure this cannot be done a lot easier using Subject<T>?
    {
        // CR(krait): What's the point of annotating private fields?
        [NotNull]
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;
        private readonly IList<IObserver<ISettingsNode>> observers;
        private readonly ConcurrentBag<IDisposable> watchers;
        private readonly IDictionary<IConfigurationSource, ISettingsNode> sourcesSettings;
        private readonly TaskSource taskSource;
        private readonly object locker;
        private ISettingsNode currentValue;
        private bool neverMerged;

        /// <summary>
        /// <para>Creates a <see cref="CombinedSource"/> instance new source using combining options.</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configuration sources to combine</param>
        /// <param name="options"></param>
        public CombinedSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources,
            SettingsMergeOptions options)
        {
            this.sources = sources;
            this.options = options;
            neverMerged = true;
            locker = new object();
            taskSource = new TaskSource();
            watchers = new ConcurrentBag<IDisposable>();
            observers = new List<IObserver<ISettingsNode>>();
            sourcesSettings = new Dictionary<IConfigurationSource, ISettingsNode>();
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="CombinedSource" /> instance new source using default combining options</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configurations</param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources.ToArray(), new SettingsMergeOptions())
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
                        SubscribeWatchers(observer);
                        currentValue = Merge(sourcesSettings.Values);
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

        private void SubscribeWatchers(IObserver<ISettingsNode> observer)
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
        }

        private ISettingsNode Merge(IEnumerable<ISettingsNode> settingses)
        {
            neverMerged = false;

            var sets = settingses as ISettingsNode[] ?? settingses.ToArray();
            if (!sets.Any() || sets.Any(s => s == null)) return null;
            if (sets.Length == 1) return sets[0];

            var merge = sets[0];
            for (var i = 1; i < sets.Length; i++)
                merge = merge.Merge(sets[i], options);

            return merge;
        }
    }
}