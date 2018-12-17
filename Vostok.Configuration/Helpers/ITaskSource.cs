using System;

namespace Vostok.Configuration.Helpers
{
    public interface ITaskSource<out T> : IDisposable    
    {
        T Get();
    }
}