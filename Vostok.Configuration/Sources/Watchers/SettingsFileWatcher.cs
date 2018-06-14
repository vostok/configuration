using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    /// Watches for changes in files
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly ConcurrentDictionary<string, IObservable<string>> Watchers =
            new ConcurrentDictionary<string, IObservable<string>>();

        /// <summary>
        /// Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<string> WatchFile([NotNull] string file) =>
            WatchFile(file, f => new SingleFileWatcher(f), true);

        internal static IObservable<string> WatchFile([NotNull] string file, Func<string, IObservable<string>> watcherCreator, bool useCache = false)
        {
            if (useCache && Watchers.TryGetValue(file, out var watcher))
                return watcher;

            watcher = watcherCreator?.Invoke(file) ?? new SingleFileWatcher(file);
            if (useCache)
                Watchers.TryAdd(file, watcher);
            return watcher;
        }
    }
}