using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Cache
{
    internal interface ISourceDataCache
    {
        SourceCacheItem<TSettings> GetLimitedCacheItem<TSettings>(IConfigurationSource source);
        SourceCacheItem<TSettings> GetPersistentCacheItem<TSettings>(IConfigurationSource source);
    }
}