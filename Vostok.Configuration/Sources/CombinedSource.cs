using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
        private readonly BehaviorSubject<RawSettings> observers;

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
            observers = new BehaviorSubject<RawSettings>(null);
            for (var i = 0; i < sources.Length; i++)
            {
                var ii = i;
                sources[i].Observe().Subscribe(settings =>
                {
                    sourcesSettings[ii] = settings;
                    observers.OnNext(Get());
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
        public RawSettings Get() => 
            sources.Length == 0 ? null : Merge(sources.Select(s => s.Get()).ToArray());

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
                list.AddRange(sets.Where((s, i) => i > 0 && s.Children != null).SelectMany(d => d.Children));

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
                var subscribtion = observers.Where(o => o != null).SubscribeSafe(observer);
                if (sourcesSettings.Any(s => s != null))
                    observer.OnNext(Get());
                return subscribtion;
            });
        }

        public void Dispose()
        {
            foreach (var source in sources)
                source.Dispose();
            observers.Dispose();
        }
    }
}