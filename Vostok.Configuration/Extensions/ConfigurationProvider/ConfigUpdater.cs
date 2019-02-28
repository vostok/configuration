using System;
using System.Linq;
using Vostok.Configuration.Binders.Extensions;

namespace Vostok.Configuration.Extensions.ConfigurationProvider
{
    internal class ConfigUpdater : IDisposable
    {
        internal readonly object Config;
        private readonly IDisposable subscription;

        public ConfigUpdater(object initialConfig, IObservable<object> observable)
        {
            Config = initialConfig;
            subscription = observable.Subscribe(UpdateConfig);
        }

        private void UpdateConfig(object newConfig)
        {
            lock (Config)
            {
                foreach (var fieldInfo in Config.GetType().GetFields().Where(pi => !pi.FieldType.IsInterface))
                    fieldInfo.SetValue(Config, fieldInfo.GetValue(newConfig));

                foreach (var propertyInfo in Config.GetType().GetProperties().Where(pi => !pi.PropertyType.IsInterface))
                    propertyInfo.ForceSetValue(Config, propertyInfo.GetValue(newConfig));
            }
        }

        public void Dispose()
        {
            subscription?.Dispose();
        }
    }
}