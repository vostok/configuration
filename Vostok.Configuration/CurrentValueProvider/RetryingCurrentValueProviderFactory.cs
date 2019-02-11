using System;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProviderFactory : ICurrentValueProviderFactory
    {
        public ICurrentValueProvider<T> Create<T>(Func<IObservable<T>> observableProvider) => new RetryingCurrentValueProvider<T>(observableProvider);
    }
}