using System;

namespace Vostok.Configuration.TaskSource
{
    internal interface ITaskSourceFactory
    {
        ITaskSource<T> Create<T>(Func<IObservable<T>> observableProvider);
    }
}