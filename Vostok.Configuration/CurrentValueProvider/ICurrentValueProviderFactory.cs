using System;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal interface ICurrentValueProviderFactory
    {
        ICurrentValueProvider<T> Create<T>(Func<IObservable<(T, Exception)>> observableProvider, HealthTracker tracker);
    }
}