using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    public class ConfigurationProviderSettings
    {
        public ISettingsBinder Binder { get; set; }
        // CR(krait): Let's remove this setting. We'll always throw when a ConfigurationProvider is badly configured (there is no source for type), and we'll always call the callback in all other cases.
        public bool ThrowExceptions { get; set; }
        public Action<Exception> OnError { get; set; }
    }
}