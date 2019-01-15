using System;

namespace Vostok.Configuration.TaskSource
{
    internal class TaskSourceFactory : ITaskSourceFactory
    {
        public ITaskSource<T> Create<T>(Func<IObservable<T>> observableProvider) => new TaskSource<T>(observableProvider);
    }
}