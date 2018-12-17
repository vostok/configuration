using System;
using System.Reactive;
using System.Threading;

namespace Vostok.Configuration.Helpers
{
    public class TaskSource<T> : ITaskSource<T>
    {
        private readonly Func<IObservable<T>> observableProvider;
        private CurrentValueObserver<T> rawValueObserver = new CurrentValueObserver<T>();

        public TaskSource(Func<IObservable<T>> observableProvider)
        {
            this.observableProvider = observableProvider;
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
            var newObserver = new CurrentValueObserver<T>();
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