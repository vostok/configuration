using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Searches subtree in <see cref="IRawSettings"/> tree.
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly IRawSettings incomeSettings;
        private readonly IConfigurationSource source;
        private readonly string[] scope;
        private readonly TaskSource taskSource;
        private IDisposable watcher;
        private IRawSettings currentValue;
        private bool firstRequest = true;

        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance for <see cref="source"/> to search in by <see cref="scope"/>
        /// <para>You can use "[n]" format in <see cref="InnerScope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="source">Source of <see cref="IRawSettings"/> tree</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            this.source = source;
            this.scope = scope;
            taskSource = new TaskSource();
        }

        /// <summary>
        /// <para>Creates a <see cref="ScopedSource"/> instance for <see cref="incomeSettings"/> to search in by <see cref="scope"/></para> 
        /// <para>You can use "[n]" format in <see cref="InnerScope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="settings">Tree to search in</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IRawSettings settings,
            [NotNull] params string[] scope)
        {
            incomeSettings = settings;
            this.scope = scope;
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets part of RawSettings tree by specified scope.
        /// </summary>
        /// <returns>Part of RawSettings tree</returns>
        public IRawSettings Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="IRawSettings"/> scoped subtree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// <para>You can get update only if you used scope by source.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<IRawSettings> Observe() =>
            Observable.Create<IRawSettings>(
                observer =>
                {
                    if (watcher == null && source != null)
                    {
                        watcher = source.Observe()
                            .Subscribe(
                                settings =>
                                {
                                    var newSettings = InnerScope(settings, scope);
                                    if (!Equals(newSettings, currentValue))
                                        currentValue = newSettings;
                                    observer.OnNext(currentValue);
                                });
                    }

                    if (watcher != null) return watcher;

                    if (firstRequest)
                    {
                        firstRequest = false;
                        currentValue = source != null ? InnerScope(source.Get(), scope) : InnerScope(incomeSettings, scope);
                    }
                    observer.OnNext(currentValue);

                    return Disposable.Empty;
                });

        public void Dispose()
        {
            watcher?.Dispose();
        }

        private static IRawSettings InnerScope(IRawSettings settings, params string[] scope)
        {
            if (scope.Length == 0)
                return settings;

            for (var i = 0; i < scope.Length; i++)
            {
                if (settings[scope[i]] != null)
                {
                    if (i == scope.Length - 1)
                        return settings[scope[i]];
                    else
                        settings = settings[scope[i]];
                }
                else if (settings.Children.Any() &&
                         scope[i].StartsWith("[") && scope[i].EndsWith("]") && scope[i].Length > 2)
                {
                    var num = scope[i].Substring(1, scope[i].Length - 2);
                    if (int.TryParse(num, out var index) && index <= settings.Children.Count() && settings.Children.ElementAt(index)?.Name == index.ToString())
                    {
                        if (i == scope.Length - 1)
                            return settings.Children.ElementAt(index);
                        else
                            settings = settings.Children.ElementAt(index);
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