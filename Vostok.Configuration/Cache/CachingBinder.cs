using System;
using System.Runtime.ExceptionServices;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Cache
{
    internal class CachingBinder : ICachingBinder
    {
        private readonly ISettingsBinder binder;

        public CachingBinder(ISettingsBinder binder)
        {
            this.binder = binder;
        }

        public TSettings Bind<TSettings>(ISettingsNode rawSettings, BindingCacheItem<TSettings> cacheItem)
        {
            var cachedValue = cacheItem.Value;
            if (cachedValue != null && Equals(cachedValue.LastBoundNode, rawSettings))
                return GetSettingsOrRethrow(cachedValue);

            CacheItemValue<TSettings> newValue;
            try
            {
                var result = binder.Bind<TSettings>(rawSettings);
                newValue = new CacheItemValue<TSettings>(rawSettings, result);
            }
            catch (Exception e)
            {
                newValue = new CacheItemValue<TSettings>(rawSettings, e);
            }

            cacheItem.Value = newValue;
            return GetSettingsOrRethrow(newValue);
        }

        private static TSettings GetSettingsOrRethrow<TSettings>(CacheItemValue<TSettings> cachedValue)
        {
            var error = cachedValue.LastError;
            if (error != null)
                ExceptionDispatchInfo.Capture(error).Throw();
            return cachedValue.LastSettings;
        }

        public class BindingCacheItem<TSettings>
        {
            public CacheItemValue<TSettings> Value { get; set; }
        }

        public class CacheItemValue<TSettings>
        {
            public ISettingsNode LastBoundNode { get; }
            public TSettings LastSettings { get; }
            public Exception LastError { get; }

            public CacheItemValue(ISettingsNode lastBoundNode, TSettings settings)
            {
                LastBoundNode = lastBoundNode;
                LastSettings = settings;
            }
            
            public CacheItemValue(ISettingsNode lastBoundNode, Exception error)
            {
                LastBoundNode = lastBoundNode;
                LastError = error;
            }
        }
    }
}