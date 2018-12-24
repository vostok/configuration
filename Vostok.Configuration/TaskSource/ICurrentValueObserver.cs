using System;

namespace Vostok.Configuration.TaskSource
{
    internal interface ICurrentValueObserver<T> : IDisposable
    {
        T Get(Func<IObservable<T>> observableProvider);
    }
}