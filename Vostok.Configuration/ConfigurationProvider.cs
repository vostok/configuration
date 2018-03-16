using System;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    // TODO(krait): Implement the default configuration provider.
    public class ConfigurationProvider : IConfigurationProvider
    {
        public TSettings Get<TSettings>()
        {
            throw new NotImplementedException();
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            throw new NotImplementedException();
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            throw new NotImplementedException();
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            throw new NotImplementedException();
        }

        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            throw new NotImplementedException();
        }
    }
}