using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    public class ConfigurationProviderSettings
    {
        public ISettingsBinder Binder { get; set; }
        public Action<Exception> OnError { get; set; }
        public int MaxTypeCacheSize { get; set; } = 10;
        public int MaxSourceCacheSize { get; set; } = 10;
    }
}