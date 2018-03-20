using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Search in RawSettings tree by dictionary keys and list indexes
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        private readonly List<IObserver<RawSettings>> observers;
        private readonly object sync;

        /// <summary>
        /// Creating scope source
        /// </summary>
        /// <param name="source">File source</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(IConfigurationSource source, params string[] scope)
        {
            this.source = source;
            this.scope = scope;

            source.Observe().Subscribe(settings =>
            {
                lock (sync)
                {
                    var scp = Get();
                    foreach (var observer in observers)
                        observer.OnNext(scp);
                }
            });

            // CR(krait): <-- and now source OnNext() fires. Boom, sync is null, observers is null.

            observers = new List<IObserver<RawSettings>>();
            sync = new object();
        }

        /// <summary>
        /// Gets part of RawSettings tree by specified scope
        /// </summary>
        /// <returns>Part of RawSettings tree</returns>
        public RawSettings Get()
        {
            var res = source.Get();
            if (scope.Length == 0)
                return res;

            for (var i = 0; i < scope.Length; i++)
            {
                if (res.ChildrenByKey != null && res.ChildrenByKey.ContainsKey(scope[i]))
                {
                    if (i == scope.Length - 1)
                        return res.ChildrenByKey[scope[i]];
                    else
                        res = res.ChildrenByKey[scope[i]];
                }
                else if (res.Children != null &&
                         scope[i].StartsWith("[") && scope[i].EndsWith("]") && scope[i].Length > 2) // TODO(krait): We must write about this [] syntax in an xml doc.
                {
                    var num = scope[i].Substring(1, scope[i].Length - 2);
                    if (int.TryParse(num, out var index) && index <= res.Children.Count)
                    {
                        if (i == scope.Length - 1)
                            return res.Children[index];
                        else
                            res = res.Children[index];
                    }
                    else
                        return null;
                }
                else
                    return null;
            }

            return null;
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                lock (sync)
                {
                    // CR(krait): Maybe it isn't necessary to store observers? See comments in ConfigurationProvider.
                    observers.Add(observer);
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
    }
}