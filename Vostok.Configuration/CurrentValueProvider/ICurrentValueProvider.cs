using System;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal interface ICurrentValueProvider<out T> : IDisposable
    {
        T Get();
    }
}