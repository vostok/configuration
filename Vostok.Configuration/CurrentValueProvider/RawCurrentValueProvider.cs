using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RawCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Func<IObservable<T>> observableProvider;
        private volatile TaskCompletionSource<T> resultSource = new TaskCompletionSource<T>();
        private IDisposable innerSubscription;

        public RawCurrentValueProvider(Func<IObservable<T>> observableProvider)
        {
            this.observableProvider = observableProvider;
        }

        public T Get()
        {
            while (innerSubscription == null)
            {
                var newSubscription = observableProvider().Subscribe(OnNextValue, OnError);

                if (Interlocked.CompareExchange(ref innerSubscription, newSubscription, null) == null)
                    break;

                newSubscription.Dispose();
            }

            return resultSource.Task.GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            var subscription = innerSubscription;
            if (subscription != null && ReferenceEquals(Interlocked.CompareExchange(ref innerSubscription, null, subscription), subscription))
                subscription.Dispose();
        }

        private static TaskCompletionSource<T> NewCompletedSource(T value)
        {
            var newSource = new TaskCompletionSource<T>();
            newSource.TrySetResult(value);
            return newSource;
        }

        private void OnError(Exception e) => resultSource.TrySetException(e);

        private void OnNextValue(T value)
        {
            if (!resultSource.TrySetResult(value))
                resultSource = NewCompletedSource(value);
        }
    }
}