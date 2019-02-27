using System;
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
                .Select(pair => pair.settings);
        }
    }
}