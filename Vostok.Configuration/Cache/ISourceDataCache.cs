using System;
using System.Collections.Generic;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Cache
{
    internal interface ISourceDataCache : IDisposable
    {
        SourceCacheItem<TSettings> GetLimitedCacheItem<TSettings>(IConfigurationSource source);

        SourceCacheItem<TSettings> GetPersistentCacheItem<TSettings>(IConfigurationSource source);

        IEnumerable<SourceCacheItem> GetAll();
    }
}