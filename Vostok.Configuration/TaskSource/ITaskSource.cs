using System;

namespace Vostok.Configuration.TaskSource
{
    internal interface ITaskSource<out T> : IDisposable
    {
        T Get();
    }
}