using System;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Helpers
{
    internal class SourceCacheItem<TSettings>
    {
        public ValueWrapper<(TSettings settings, Exception error)> LastValue { get; set; }
        public CachingBinder.BindingCacheItem<TSettings> BindingCacheItem { get; } = new CachingBinder.BindingCacheItem<TSettings>();
    }
}