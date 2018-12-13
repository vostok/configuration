namespace Vostok.Configuration.Helpers
{
    internal class TaskSourceFactory : ITaskSourceFactory
    {
        public ITaskSource<T> Create<T>() => new TaskSource<T>();
    }
}