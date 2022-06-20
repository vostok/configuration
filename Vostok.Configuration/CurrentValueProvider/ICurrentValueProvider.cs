using System;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal interface ICurrentValueProvider<out T> : IDisposable
    {
        T Get();
        
        HealthTracker HealthTracker { get; }
    }
}