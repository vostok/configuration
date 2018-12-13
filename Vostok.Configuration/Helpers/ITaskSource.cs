using System;

namespace Vostok.Configuration.Helpers
{
    public interface ITaskSource<T>
    {
        T Get(Func<IObservable<T>> observableProvider);
    }
}