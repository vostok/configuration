using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Searches subtree in <see cref="RawSettings"/> tree.
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly BehaviorSubject<RawSettings> observers;
        private readonly IDisposable watcher;
        //        private readonly IConfigurationSource source;
        //        private readonly RawSettings settings;
        //        private readonly string[] scope;
        private RawSettings currentSettings;

        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance for <see cref="source"/> to search in by <see cref="scope"/>
        /// <para>You can use "[n]" format in <see cref="Scope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="source">Source of <see cref="RawSettings"/> tree</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            observers = new BehaviorSubject<RawSettings>(currentSettings);
            currentSettings = Scope(source.Get(), scope);
            watcher = source.Observe()
                .Subscribe(
                    settings =>
                    {
                        var newSettings = Scope(settings, scope);
                        if (!Equals(newSettings, currentSettings))
                        {
                            currentSettings = newSettings;
                            observers.OnNext(currentSettings);
                        }
                    });
        }

        /// <summary>
        /// <para>Creates a <see cref="ScopedSource"/> instance for <see cref="settings"/> to search in by <see cref="scope"/></para> 
        /// <para>You can use "[n]" format in <see cref="Scope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="settings">Tree to search in</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] RawSettings settings,
            [NotNull] params string[] scope)
        {
            observers = new BehaviorSubject<RawSettings>(currentSettings);
            currentSettings = Scope(settings, scope);
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets part of RawSettings tree by specified scope.
        /// </summary>
        /// <returns>Part of RawSettings tree</returns>
        public RawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="RawSettings"/> scoped subtree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// <para>You can get update only if you used scope by source.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<RawSettings> Observe() =>
            Observable.Create<RawSettings>(
                observer => observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            observers.Dispose();
            watcher?.Dispose();
        }

        private RawSettings Scope(RawSettings settings, params string[] scope)
        {
            if (scope.Length == 0)
                return settings;

            for (var i = 0; i < scope.Length; i++)
            {
                if (settings.ChildrenByKey != null && settings.ChildrenByKey.ContainsKey(scope[i]))
                {
                    if (i == scope.Length - 1)
                        return settings.ChildrenByKey[scope[i]];
                    else
                        settings = settings.ChildrenByKey[scope[i]];
                }
                else if (settings.Children != null &&
                         scope[i].StartsWith("[") && scope[i].EndsWith("]") && scope[i].Length > 2)
                {
                    var num = scope[i].Substring(1, scope[i].Length - 2);
                    if (int.TryParse(num, out var index) && index <= settings.Children.Count)
                    {
                        if (i == scope.Length - 1)
                            return settings.Children[index];
                        else
                            settings = settings.Children[index];
                    }
                    else
                        return null;
                }
                else
                    return null;
            }

            return null;
        }
    }
}