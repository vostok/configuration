using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// A source that combines settings from several other sources, resolving possible conflicts in favor of sources which one came earlier in the list
    /// </summary>
    public class CombinedSource : IConfigurationSource
    {
        private readonly ListCombineOptions listCombineOptions;
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly List<IDisposable> watchers;
        private readonly IDictionary<IConfigurationSource, RawSettings> sourcesSettings;
        private RawSettings currentSettings;

        /// <summary>
        /// <para>Creates a <see cref="CombinedSource"/> instance new source using combining options.</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configuration sources to combine</param>
        /// <param name="listCombineOptions">Options for lists combining</param>
        public CombinedSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources,
            ListCombineOptions listCombineOptions)
        {
            this.listCombineOptions = listCombineOptions;
            sourcesSettings = new Dictionary<IConfigurationSource, RawSettings>();
            observers = new BehaviorSubject<RawSettings>(currentSettings);
            watchers = new List<IDisposable>(sources.Count);
            foreach (var source in sources)
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
            MergeIntoCurrentSettings(sourcesSettings.Values.ToArray());
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="CombinedSource" /> instance new source using default combining options</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configurations</param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources.ToArray(), ListCombineOptions.FirstOnly)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously combined configurations. Null if sources where not specified.
        /// </summary>
        /// <returns>Combine as RawSettings tree</returns>
        public RawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="RawSettings"/> changes in any of sources.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<RawSettings> Observe() =>
            Observable.Create<RawSettings>(
                observer => observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            foreach (var watcher in watchers)
                watcher.Dispose();
            observers.Dispose();
        }

        private void MergeIntoCurrentSettings(IEnumerable<RawSettings> settingses)
        {
            var sets = settingses as RawSettings[] ?? settingses.ToArray();
            currentSettings = !sets.Any() ? null : Merge(sets);
        }

        private RawSettings Merge(IEnumerable<RawSettings> settingses)
        {
            var sets = settingses as RawSettings[] ?? settingses.ToArray();
            if (sets == null || sets.Length == 0) return null;
            var dict = sets[0].ChildrenByKey?.ToDictionary(pair => pair.Key, pair => pair.Value);
            var list = sets[0].Children?.ToList();
            var strValue = sets[0].Value;

            if (dict != null)
            {
                var allDicts = sets.Where(s => s.ChildrenByKey != null).SelectMany(d => d.ChildrenByKey).ToArray();
                var notFirstDicts = sets.Where((s, i) => i > 0 && s.ChildrenByKey != null).SelectMany(d => d.ChildrenByKey).ToArray();
                foreach (var pair in notFirstDicts.Where(d => !dict.ContainsKey(d.Key) && d.Value.ChildrenByKey == null && d.Value.Children == null))
                    dict.Add(pair.Key, pair.Value);

                var complexDicts = notFirstDicts.Where(d => d.Value.ChildrenByKey != null || d.Value.Children != null).ToArray();
                var complexKeys = complexDicts.Select(d => d.Key).Distinct().ToArray();
                foreach (var key in complexKeys)
                    dict[key] = Merge(allDicts.Where(d => d.Key == key).Select(d => d.Value).ToArray());
            }

            if (list != null && listCombineOptions == ListCombineOptions.UnionAll)
                list.AddRange(sets.Where((s, i) => i > 0 && s.Children != null).SelectMany(d => d.Children));

            return new RawSettings(dict, list, strValue);
        }
    }
}