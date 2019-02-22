using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RawCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Lazy<IObservable<T>> observable;
        private volatile TaskCompletionSource<T> resultSource = new TaskCompletionSource<T>();
        private IDisposable innerSubscription;

        public RawCurrentValueProvider(Func<IObservable<T>> observableProvider)
        {
            observable = new Lazy<IObservable<T>>(observableProvider, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public T Get()
        {
            // We check for the existing value here to avoid subscription leak when Get() is called after Dispose().
            // The following sequence of calls is safe: Get(); Dispose(); Get();
            if (innerSubscription == null && !resultSource.Task.IsCompleted)
                Subscribe();

            return resultSource.Task.GetAwaiter().GetResult();
        }

        public void Dispose() => Interlocked.Exchange(ref innerSubscription, null)?.Dispose();

        private static TaskCompletionSource<T> NewCompletedSource(T value)
        {
            var newSource = new TaskCompletionSource<T>();
            newSource.TrySetResult(value);
            return newSource;
        }

        private void OnError(Exception e)
        {
            if (resultSource.TrySetException(e))
                Dispose();
        }

        private void OnNextValue(T value)
        {
            if (!resultSource.TrySetResult(value))
                resultSource = NewCompletedSource(value);
        }

        private void Subscribe()
        {
            while (innerSubscription == null)
            {
                var newSubscription = observable.Value.Subscribe(OnNextValue, OnError);

                if (Interlocked.CompareExchange(ref innerSubscription, newSubscription, null) == null)
                    break;

                newSubscription.Dispose();
            }
        }
    }
}