using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    public class ConfigurationProviderSettings
    {
        public ISettingsBinder Binder { get; set; }
        public Action<Exception> ErrorCallBack { get; set; }
        public int MaxTypeCacheSize { get; set; } = 10;
        public int MaxSourceCacheSize { get; set; } = 10;
    }
}