using System;
using System.Threading;

namespace Vostok.Configuration.TaskSource
{
    internal class TaskSource<T> : ITaskSource<T>
    {
        private readonly Func<IObservable<T>> observableProvider;
        private readonly Func<ICurrentValueObserver<T>> currentValueObserverFactory;
        private ICurrentValueObserver<T> rawValueObserver;

        public TaskSource(Func<IObservable<T>> observableProvider)
            : this(observableProvider, () => new CurrentValueObserver<T>())
        {
        }

        internal TaskSource(Func<IObservable<T>> observableProvider, Func<ICurrentValueObserver<T>> currentValueObserverFactory)
        {
            this.observableProvider = observableProvider;
            this.currentValueObserverFactory = currentValueObserverFactory;
            rawValueObserver = currentValueObserverFactory();
        }

        public T Get()
        {
            try
            {
                return rawValueObserver.Get(observableProvider);
            }
            catch
            {
                ReplaceObserver();
                return rawValueObserver.Get(observableProvider);
            }
        }

        private void ReplaceObserver()
        {
            var currentObserver = rawValueObserver;
            var newObserver = currentValueObserverFactory();
            if (Interlocked.CompareExchange(ref rawValueObserver, newObserver, currentObserver) != currentObserver)
                newObserver.Dispose();
            currentObserver.Dispose();
        }

        public void Dispose()
        {
            rawValueObserver?.Dispose();
        }
    }
}