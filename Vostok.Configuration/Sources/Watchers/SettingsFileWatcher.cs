using System;
using System.Collections.Concurrent;
using System.Text;
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
        /// <param name="encoding"></param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<string> WatchFile([NotNull] string file, Encoding encoding = null) =>
            WatchFile(file, encoding, (f, e) => new SingleFileWatcher(f, e), true);

        internal static IObservable<string> WatchFile([NotNull] string file, Encoding encoding, Func<string, Encoding, IObservable<string>> watcherCreator, bool useCache = false)
        {
            if (useCache && Watchers.TryGetValue(file, out var watcher))
                return watcher;

            encoding = encoding ?? Encoding.UTF8;
            watcher = watcherCreator?.Invoke(file, encoding) ?? new SingleFileWatcher(file, encoding);
            if (useCache)
                Watchers.TryAdd(file, watcher);
            return watcher;
        }
    }
}