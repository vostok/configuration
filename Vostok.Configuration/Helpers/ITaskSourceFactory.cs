namespace Vostok.Configuration.Helpers
{
    internal interface ITaskSourceFactory
    {
        ITaskSource<T> Create<T>();
    }
}