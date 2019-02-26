using System;
using System.Runtime.ExceptionServices;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Cache
{
    internal class CachingBinder : ICachingBinder
    {
        private readonly ISettingsBinder binder;

        public CachingBinder(ISettingsBinder binder)
        {
            this.binder = binder;
        }

        public TSettings Bind<TSettings>(ISettingsNode rawSettings, IBindingCacheItem<TSettings> cacheItem)
        {
            var cachedValue = cacheItem.BindingCacheValue;
            if (cachedValue != null && Equals(cachedValue.LastBoundNode, rawSettings))
                return GetSettingsOrRethrow(cachedValue);

            try
            {
                var result = binder.Bind<TSettings>(rawSettings);
                cacheItem.BindingCacheValue = new BindingCacheValue<TSettings>(rawSettings, result);
            }
            catch (Exception e)
            {
                cacheItem.BindingCacheValue = new BindingCacheValue<TSettings>(rawSettings, e);
            }

            return GetSettingsOrRethrow(cacheItem.BindingCacheValue);
        }

        private static TSettings GetSettingsOrRethrow<TSettings>(BindingCacheValue<TSettings> cachedValue)
        {
            var error = cachedValue.LastError;
            if (error != null)
                ExceptionDispatchInfo.Capture(error).Throw();
            return cachedValue.LastSettings;
        }
    }
}