using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    public class ConfigurationProviderSettings
    {
        public ISettingsBinder Binder { get; set; }
        public bool ThrowExceptions { get; set; }
        public Action<Exception> OnError { get; set; }
    }
}