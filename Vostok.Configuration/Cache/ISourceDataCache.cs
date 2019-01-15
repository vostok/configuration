using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Cache
{
    internal interface ISourceDataCache : IDisposable
    {
        SourceCacheItem<TSettings> GetLimitedCacheItem<TSettings>(IConfigurationSource source);

        SourceCacheItem<TSettings> GetPersistentCacheItem<TSettings>(IConfigurationSource source);
    }
}