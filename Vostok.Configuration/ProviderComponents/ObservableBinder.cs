using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Cache;
using Vostok.Configuration.EqualityComparers;

namespace Vostok.Configuration.ProviderComponents
{
    internal class ObservableBinder : IObservableBinder
    {
        private readonly ICachingBinder binder;

        public ObservableBinder(ICachingBinder binder)
        {
            this.binder = binder;
        }

        public IObservable<(TSettings settings, Exception error)> SelectBound<TSettings>(IObservable<(ISettingsNode settings, Exception error)> sourceObservable, Func<SourceCacheItem<TSettings>> cacheItemProvider)
        {
            return sourceObservable
                .Select(
                    sourceValue =>
                    {
                        var cacheItem = cacheItemProvider();

                        var resultError = sourceValue.error;
                        if (resultError == null)
                        {
                            try
                            {
                                var boundSettings = binder.Bind(sourceValue.settings, cacheItem.BindingCacheItem);
                                return Notification.CreateOnNext((boundSettings, null as Exception));
                            }
                            catch (Exception error)
                            {
                                resultError = error;
                            }
                        }

                        var cachedValue = cacheItem.LastValue;

                        return cachedValue == null
                            ? Notification.CreateOnError<(TSettings, Exception)>(resultError)
                            : Notification.CreateOnNext((cachedValue.Value.settings, resultError));
                    })
                .Dematerialize()
                .DistinctUntilChanged(new TupleEqualityComparer<TSettings, Exception>(EqualityComparer<TSettings>.Default, new ExceptionEqualityComparer()))
                .Do(value => cacheItemProvider().LastValue = value);
        }
    }
}