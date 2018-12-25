using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    public class ConfigurationProviderSettings
    {
        public ISettingsBinder Binder { get; set; }

        public Action<Exception> ErrorCallback { get; set; }

        public int MaxSourceCacheSize { get; set; } = 10;
    }
}