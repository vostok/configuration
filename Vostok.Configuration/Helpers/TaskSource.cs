using System;
using System.Threading;

namespace Vostok.Configuration.Helpers
{
    public class TaskSource<T> : IDisposable, ITaskSource<T>
    {
        private CurrentValueObserver<T> rawValueObserver;

        public TaskSource() => rawValueObserver = new CurrentValueObserver<T>();

        public T Get(Func<IObservable<T>> observableProvider)
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