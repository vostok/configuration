using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RawCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Lazy<IObservable<(T, Exception)>> observable;
        private readonly Action<Exception> errorCallback;

        private volatile TaskCompletionSource<T> resultSource = new TaskCompletionSource<T>();
        private volatile IDisposable innerSubscription;

        public RawCurrentValueProvider(Func<IObservable<(T, Exception)>> observableProvider, Action<Exception> errorCallback)
        {
            this.errorCallback = errorCallback;

            observable = new Lazy<IObservable<(T, Exception)>>(observableProvider, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public T Get()
        {
            // We check for the existing value here to avoid subscription leak when Get() is called after Dispose().
            // The following sequence of calls is safe: Get(); Dispose(); Get();
            if (innerSubscription == null && !resultSource.Task.IsCompleted)
                Subscribe();

            return resultSource.Task.GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref innerSubscription, null)?.Dispose();
            resultSource.TrySetException(new ObjectDisposedException(nameof(RawCurrentValueProvider<T>)));
        }

        private static TaskCompletionSource<T> NewCompletedSource(T value)
        {
            var newSource = new TaskCompletionSource<T>();
            newSource.TrySetResult(value);
            return newSource;
        }

        private void OnError(Exception error)
        {
            if (resultSource.TrySetException(error))
            {
                Dispose();
            }
            else errorCallback(error);
        }

        private void OnNextValue((T settings, Exception error) value)
        {
            if (value.error != null)
            {
                OnError(value.error);
            }
            else
            {
                if (!resultSource.TrySetResult(value.settings))
                    resultSource = NewCompletedSource(value.settings);
            }
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