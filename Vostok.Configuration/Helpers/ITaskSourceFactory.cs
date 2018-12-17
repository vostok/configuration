using System;

namespace Vostok.Configuration.Helpers
{
    internal interface ITaskSourceFactory
    {
        ITaskSource<T> Create<T>(Func<IObservable<T>> observableProvider);
    }
}