using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Search in RawSettings tree by dictionary keys and list indexes
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly RawSettings settings;
        private readonly string[] scope;

        /// <summary>
        /// Creating scope source
        /// </summary>
        /// <param name="source">File source</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(IConfigurationSource source, params string[] scope)
        {
            this.source = source;
            this.scope = scope;
        }

        public ScopedSource(RawSettings settings, params string[] scope)
        {
            this.settings = settings;
            this.scope = scope;
        }

        /// <summary>
        /// Gets part of RawSettings tree by specified scope.
        /// You can use "[n]" format to get n-th index of list.
        /// </summary>
        /// <returns>Part of RawSettings tree</returns>
        public RawSettings Get()
        {
            var res = settings ?? source.Get();
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

        public IObservable<RawSettings> Observe() => source.Observe().Select(s => Get());

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}