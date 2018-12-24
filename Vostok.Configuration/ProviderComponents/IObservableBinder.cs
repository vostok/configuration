using System;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Cache;

namespace Vostok.Configuration.ProviderComponents
{
    internal interface IObservableBinder
    {
        IObservable<(TSettings settings, Exception error)> SelectBound<TSettings>(IObservable<(ISettingsNode settings, Exception error)> sourceObservable, Func<SourceCacheItem<TSettings>> cacheItemProvider);
    }
}