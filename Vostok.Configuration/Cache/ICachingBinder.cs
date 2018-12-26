using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Cache
{
    internal interface ICachingBinder
    {
        TSettings Bind<TSettings>(ISettingsNode rawSettings, IBindingCacheItem<TSettings> cacheItem);
    }
}