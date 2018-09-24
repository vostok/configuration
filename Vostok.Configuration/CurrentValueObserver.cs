using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Configuration
{
    internal class CurrentValueObserver<T>
    {
        private volatile TaskCompletionSource<T> resultSource = new TaskCompletionSource<T>();
        private IDisposable innerSubscription;

        public T Get(IObservable<T> observable)
        {
            while (innerSubscription == null)
            {
                var newSubscription = observable.Subscribe(OnNextValue, OnError);

                if (Interlocked.CompareExchange(ref innerSubscription, newSubscription, null) == null)
                    break;

                newSubscription.Dispose();
            }

            return resultSource.Task.GetAwaiter().GetResult();
        }

        private void OnError(Exception e) =>
            resultSource.TrySetException(e);

        private void OnNextValue(T value)
        {
            if (!resultSource.TrySetResult(value))
                resultSource = NewCompletedSource(value);
        }

        private static TaskCompletionSource<T> NewCompletedSource(T value)
        {
            var newSource = new TaskCompletionSource<T>();
            newSource.TrySetResult(value);
            return newSource;
        }
    }
}