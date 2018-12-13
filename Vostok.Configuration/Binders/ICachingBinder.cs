using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal interface ICachingBinder
    {
        TSettings Bind<TSettings>(ISettingsNode rawSettings, CachingBinder.BindingCacheItem<TSettings> cacheItem);
    }
}