using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Watches for changes in files
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly ConcurrentDictionary<string, SingleFileWatcher> Watchers =
            new ConcurrentDictionary<string, SingleFileWatcher>();
        private static readonly object Locker = new object();

        /// <summary>
        /// Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<string> WatchFile([NotNull] string file)
        {
            if (Watchers.TryGetValue(file, out var watcher) && watcher != null)
                return watcher;

            lock (Locker)
                watcher = new SingleFileWatcher(file);
            Watchers.TryAdd(file, watcher);
            return watcher;
        }
    }
}