using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Vostok.Configuration.Extensions
{
    internal static class ObservableExtensions
    {
        public static IObservable<TSettings> SendErrorsToCallback<TSettings>(this IObservable<(TSettings settings, Exception error)> observable, Action<Exception> callback)
        {
            return observable
                .Do(
                    pair =>
                    {
                        if (pair.error != null)
                            callback(pair.error);
                    })
                .Where(pair => pair.error == null)
                .Select(pair => pair.settings)
                .DistinctUntilChanged(EqualityComparer<TSettings>.Default);
        }
    }
}