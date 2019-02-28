using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Cache;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.ObservableBinding
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
                .DistinctUntilChanged()
                .Select(
                    sourceValue =>
                    {
                        var cacheItem = cacheItemProvider();

                        var resultError = sourceValue.error;
                        if (resultError == null)
                        {
                            try
                            {
                                var boundSettings = binder.Bind(sourceValue.settings, cacheItem);

                                return (hasValidSettings: true, settings: boundSettings, error: null as Exception);
                            }
                            catch (Exception error)
                            {
                                resultError = error;
                            }
                        }

                        return (hasValidSettings: false, settings: default, error: resultError);
                    })
                .Scan(
                    (hasValidSettings: false, settings: default(TSettings), error: null as Exception),
                    (previousValue, currentValue) =>
                    {
                        if (currentValue.error != null && previousValue.hasValidSettings)
                            return (true, previousValue.settings, currentValue.error);

                        return currentValue;
                    })
                .Select(value => (value.settings, value.error))
                .DistinctUntilChanged(new TupleEqualityComparer<TSettings, Exception>(EqualityComparer<TSettings>.Default, new ExceptionEqualityComparer()));
        }
    }
}