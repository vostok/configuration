using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Helpers
{
    internal class ObservableBinder : IObservableBinder
    {
        private readonly CachingBinder binder;

        public ObservableBinder(CachingBinder binder)
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
                        Exception bindingError;
                        try
                        {
                            var boundSettings = binder.Bind(sourceValue.settings, cacheItem.BindingCacheItem);
                            return Notification.CreateOnNext((boundSettings, sourceValue.error));
                        }
                        catch (Exception error)
                        {
                            bindingError = error;
                        }
                        
                        var cachedValue = cacheItem.LastValue;
                        var resultError = sourceValue.error != null ? new AggregateException(sourceValue.error, bindingError) : bindingError;
                        
                        if (cachedValue == null)
                            return Notification.CreateOnError<(TSettings, Exception)>(resultError);
                        
                        if (!ExceptionsComparer.Equals(resultError, cachedValue.Value.error))
                            return Notification.CreateOnNext((cachedValue.Value.settings, resultError));

                        return null;
                    })
                .Where(notification => notification != null)
                .Select(notification =>
                {
                    if (notification.Kind != NotificationKind.OnError)
                        return notification.Value;
                    ExceptionDispatchInfo.Capture(notification.Exception).Throw();
                    return default;
                })
                .Do(value => cacheItemProvider().LastValue = value);
        }
    }
}