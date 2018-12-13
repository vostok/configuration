using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Helpers
{
    internal interface IObservableBinder
    {
        IObservable<(TSettings settings, Exception error)> SelectBound<TSettings>(IObservable<(ISettingsNode settings, Exception error)> sourceObservable, Func<SourceCacheItem<TSettings>> cacheItemProvider);
    }
}