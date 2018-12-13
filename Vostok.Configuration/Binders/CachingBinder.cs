using System;
using System.Runtime.ExceptionServices;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
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

            (TSettings settings, Exception error) result;
            try
            {
                result = (binder.Bind<TSettings>(rawSettings), null);
            }
            catch (Exception e)
            {
                result = (default, e);
            }

            var newValue = new CacheItemValue<TSettings>(rawSettings, result);
            cacheItem.Value = newValue;
            return GetSettingsOrRethrow(newValue);
        }

        private static TSettings GetSettingsOrRethrow<TSettings>(CacheItemValue<TSettings> cachedValue)
        {
            var error = cachedValue.LastBindResult.error;
            if (error != null)
                ExceptionDispatchInfo.Capture(error).Throw();
            return cachedValue.LastBindResult.settings;
        }

        public class BindingCacheItem<TSettings>
        {
            public CacheItemValue<TSettings> Value { get; set; }
        }

        public class CacheItemValue<TSettings>
        {
            public ISettingsNode LastBoundNode { get; }
            public (TSettings settings, Exception error) LastBindResult { get; }

            public CacheItemValue(ISettingsNode lastBoundNode, (TSettings settings, Exception error) lastBindResult)
            {
                LastBoundNode = lastBoundNode;
                LastBindResult = lastBindResult;
            }
        }
    }
}