using System;

namespace Vostok.Configuration.TaskSource
{
    public interface ITaskSource<out T> : IDisposable    
    {
        T Get();
    }
}