using System;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProviderFactory : ICurrentValueProviderFactory
    {
        private readonly TimeSpan retryCooldown;

        public RetryingCurrentValueProviderFactory(TimeSpan retryCooldown)
        {
            this.retryCooldown = retryCooldown;
        }

        public ICurrentValueProvider<T> Create<T>(Func<IObservable<(T, Exception)>> observableProvider, Action<Exception> errorCallback) 
            => new RetryingCurrentValueProvider<T>(observableProvider, retryCooldown, errorCallback);
    }
}