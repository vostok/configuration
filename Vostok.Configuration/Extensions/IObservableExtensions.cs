using System;
using System.Threading.Tasks;

namespace Vostok.Configuration.Extensions
{
    public static class IObservableExtensions
    {
        public static IDisposable SubscribeTo<T>(this IObservable<T> source, Action<T> onNext) =>
            source.Subscribe(onNext);

        public static IDisposable SubscribeTo<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError) =>
            source.Subscribe(onNext, e => Task.Run(() => onError.Invoke(e)));
    }
}