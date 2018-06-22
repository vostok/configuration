using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    internal class TaskSource
    {
        private readonly ConcurrentDictionary<Type, object> typedValueObservers;

        private CurrentValueObserver<ISettingsNode> rawValueObserver;

        public TaskSource()
        {
            rawValueObserver = new CurrentValueObserver<ISettingsNode>();
            typedValueObservers = new ConcurrentDictionary<Type, object>();
        }

        public ISettingsNode Get(IObservable<ISettingsNode> observable)
        {
            try
            {
                return rawValueObserver.Get(observable);
            }
            catch
            {
                rawValueObserver = new CurrentValueObserver<ISettingsNode>();
                throw;
            }
        }

        public T Get<T>(IObservable<T> observable)
        {
            var observer = (CurrentValueObserver<T>)typedValueObservers.GetOrAdd(typeof(T), _ => new CurrentValueObserver<T>());

            try
            {
                return observer.Get(observable);
            }
            catch
            {
                typedValueObservers.TryRemove(typeof(T), out _);
                throw;
            }
        }
    }

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