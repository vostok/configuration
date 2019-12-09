using System;
using System.Threading;
using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers
{
    internal static class DefaultConfigurationProvider
    {
        private static readonly object Sync = new object();

        private static volatile ConfigurationProvider provider;

        [NotNull]
        public static ConfigurationProvider Get()
        {
            if (provider != null)
                return provider;

            lock (Sync)
                return provider ?? (provider = Create());
        }

        public static bool TryConfigure([NotNull] ConfigurationProvider newProvider, bool canOverwrite)
        {
            if (newProvider == null)
                throw new ArgumentNullException(nameof(newProvider));

            lock (Sync)
            {
                if (!canOverwrite && provider != null)
                    return false;

                Interlocked.Exchange(ref provider, newProvider);
                return true;
            }
        }

        private static ConfigurationProvider Create()
            => new ConfigurationProvider(new ConfigurationProviderSettings { MaxSourceCacheSize = 500 });
    }
}
