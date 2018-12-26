namespace Vostok.Configuration.Cache
{
    internal interface IBindingCacheItem<TSettings>
    {
        BindingCacheValue<TSettings> BindingCacheValue { get; set; }
    }
}