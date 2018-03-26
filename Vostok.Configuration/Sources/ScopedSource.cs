using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Search in RawSettings tree by dictionary keys and list indexes
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        private readonly BehaviorSubject<RawSettings> observers;
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

            observers = new BehaviorSubject<RawSettings>(null);
            sync = new object();
            source.Observe().Subscribe(settings => observers.OnNext(Get()));
        }

        /// <summary>
        /// Gets part of RawSettings tree by specified scope.
        /// You can use "[n]" format to get n-th index of list.
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
                         scope[i].StartsWith("[") && scope[i].EndsWith("]") && scope[i].Length > 2)
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
                var subscribtion = observers.Where(o => o != null).SubscribeSafe(observer);
                observer.OnNext(Get());
                return subscribtion;
            });
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}