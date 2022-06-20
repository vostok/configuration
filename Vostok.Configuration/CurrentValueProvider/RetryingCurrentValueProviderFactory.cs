using System;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProviderFactory : ICurrentValueProviderFactory
    {
        private readonly TimeSpan retryCooldown;
        private readonly Action<Exception> errorCallback;

        public RetryingCurrentValueProviderFactory(TimeSpan retryCooldown, Action<Exception> errorCallback)
        {
            this.retryCooldown = retryCooldown;
            this.errorCallback = errorCallback;
        }

        public ICurrentValueProvider<T> Create<T>(Func<IObservable<(T, Exception)>> observableProvider, HealthTracker healthTracker)
            => new RetryingCurrentValueProvider<T>(observableProvider, retryCooldown, errorCallback, healthTracker);
    }
}