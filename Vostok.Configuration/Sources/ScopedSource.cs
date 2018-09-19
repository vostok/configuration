using System;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Searches subtree in <see cref="ISettingsNode"/> tree.
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly ISettingsNode incomeSettings;
        private readonly IConfigurationSource source;
        private readonly string[] scope;
        private readonly TaskSource taskSource;
        private readonly object locker;
        private (ISettingsNode settings, Exception error) currentValue;
        private bool firstRequest = true;

        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance for <see cref="source"/> to search in by <see cref="scope"/>
        /// <para>You can use "[n]" format in <see cref="InnerScope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="source">Source of <see cref="ISettingsNode"/> tree</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            this.source = source;
            this.scope = scope;
            locker = new object();
            taskSource = new TaskSource();
        }

        /// <summary>
        /// <para>Creates a <see cref="ScopedSource"/> instance for <see cref="incomeSettings"/> to search in by <see cref="scope"/></para> 
        /// <para>You can use "[n]" format in <see cref="InnerScope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="settings">Tree to search in</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] ISettingsNode settings,
            [NotNull] params string[] scope)
        {
            incomeSettings = settings;
            this.scope = scope;
            locker = new object();
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets part of RawSettings tree by specified scope.
        /// </summary>
        /// <returns>Part of RawSettings tree</returns>
        public ISettingsNode Get() => taskSource.Get(Observe()).settings;

        private static ISettingsNode InnerScope(ISettingsNode settings, params string[] scope)
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
                         scope[i].StartsWith("[") && scope[i].EndsWith("]") && scope[i].Length > 2 && settings is ArrayNode)
                {
                    var num = scope[i].Substring(1, scope[i].Length - 2);
                    if (int.TryParse(num, out var index) && index < settings.Children.Count())
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

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            if (source != null)
                return source.Observe()
                    .Select(
                        pair =>
                        {
                            lock (locker)
                            {
                                var newSettings = (InnerScope(pair.settings, scope), null as Exception);
                                if (!Equals(newSettings, currentValue) || firstRequest)
                                {
                                    firstRequest = false;
                                    currentValue = newSettings;
                                }

                                return currentValue;
                            }
                        });

            if (firstRequest)
            {
                currentValue = (InnerScope(incomeSettings, scope), null);
                firstRequest = false;
            }

            return Observable.Return(currentValue);
        }
    }
}