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
        private readonly BehaviorSubject<IRawSettings> observers;
        private readonly List<IDisposable> watchers;
        private readonly IDictionary<IConfigurationSource, IRawSettings> sourcesSettings;
        private IRawSettings currentSettings;

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
            sourcesSettings = new Dictionary<IConfigurationSource, IRawSettings>();
            observers = new BehaviorSubject<IRawSettings>(currentSettings);
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
        public IRawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="IRawSettings"/> changes in any of sources.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<IRawSettings> Observe() =>
            Observable.Create<IRawSettings>(
                observer => observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            foreach (var watcher in watchers)
                watcher.Dispose();
            observers.Dispose();
        }

        private void MergeIntoCurrentSettings(IEnumerable<IRawSettings> settingses)
        {
            var sets = settingses as RawSettings[] ?? settingses.ToArray();
            currentSettings = !sets.Any() ? null : Merge(sets);
        }

        private static IDictionary<string, IRawSettings> GetDictionary(IEnumerable<IRawSettings> list)
        {
            var sets = list as IRawSettings[] ?? list.ToArray();
            var result = new Dictionary<string, IRawSettings>(sets.Count());
            foreach (var child in sets)
                result.Add(child.Name, child);
            return result;
        }

        private static RawSettings Merge(IEnumerable<IRawSettings> settingses)
        {
            var sets = settingses as RawSettings[] ?? settingses.ToArray();
            if (sets == null || sets.Length == 0) return null;
            var dict = GetDictionary(sets[0].Children);
            var strValue = sets[0].Value;
            var name = sets[0].Name;

            var allDicts = sets.SelectMany(d => GetDictionary(d.Children)).ToArray();
            var notFirstDicts = sets.Where((s, i) => i > 0).SelectMany(d => GetDictionary(d.Children)).ToArray();
            foreach (var pair in notFirstDicts.Where(d => !dict.ContainsKey(d.Key)))
                dict.Add(pair.Key, pair.Value);

            var complexDicts = notFirstDicts.ToArray();
            var complexKeys = complexDicts.Select(d => d.Key).Distinct().ToArray();
            foreach (var key in complexKeys)
                dict[key] = Merge(allDicts.Where(d => d.Key == key).Select(d => d.Value).ToArray());
    
            return new RawSettings(dict.ToOrderedDictionary(p => p.Key, p => p.Value), name, strValue);
        }
    }
}