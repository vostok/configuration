using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// A source that combines settings from several other sources, resolving possible conflicts in favor of sources that come earlier in the list
    /// </summary>
    public class CombinedSource : IConfigurationSource
    {
        private readonly IConfigurationSource[] sources;
        private readonly ListCombineOptions listCombineOptions;
        private readonly RawSettings[] sourcesSettings;

        private readonly List<IObserver<RawSettings>> observers;
        private readonly object sync;

        /// <summary>
        /// Creating new source with list of configurations to combine
        /// </summary>
        /// <param name="sources">Configurations</param>
        /// <param name="listCombineOptions">Options for list combining</param>
        public CombinedSource(IConfigurationSource[] sources, ListCombineOptions listCombineOptions)
        {
            this.sources = sources;
            this.listCombineOptions = listCombineOptions;
            sourcesSettings = new RawSettings[sources.Length];
            observers = new List<IObserver<RawSettings>>();
            sync = new object();
            for (var i = 0; i < sources.Length; i++)
            {
                var i1 = i;
                sources[i].Observe().Subscribe(settings =>
                {
                    lock (sync)
                    {
                        sourcesSettings[i1] = settings;
                        var merge = Get();
                        foreach (var observer in observers)
                            observer.OnNext(merge);
                    }
                });
            }
        }

        /// <summary>
        /// Creating new source with list of configurations to combine
        /// </summary>
        /// <param name="sources">Configurations</param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources.ToArray(), ListCombineOptions.FirstOnly)
        {
        }

        /// <summary>
        /// Returns combine of configurations
        /// </summary>
        /// <returns>Combine as RawSettings tree</returns>
        public RawSettings Get()
        {
            if (sources.Length == 0) return null;
            var sets = sources.Select(s => s.Get()).ToArray();
            return Merge(sets);
        }

        private RawSettings Merge(params RawSettings[] sets)
        {
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
                foreach (var value in sets.Where((s, i) => i > 0 && s.Children != null).SelectMany(d => d.Children))
                    list.Add(value);

            return new RawSettings(dict, list, strValue);
        }

        /// <summary>
        /// Watches changes of any of the sources
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(observer);

                    if (sourcesSettings.Any(s => s != null))
                        observer.OnNext(Get());
                }
                return Disposable.Create(() =>
                {
                    lock (sync)
                    {
                        observers.Remove(observer);
                    }
                });
            });
        }

        public void Dispose()
        {
            foreach (var source in sources)
                source.Dispose();
        }
    }
}