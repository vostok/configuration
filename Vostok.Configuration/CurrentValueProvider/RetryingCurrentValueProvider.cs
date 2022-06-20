using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Func<ICurrentValueProvider<T>> currentValueProviderFactory;
        private readonly TimeSpan retryCooldown;

        private ICurrentValueProvider<T> currentValueProvider;
        private object cooldownToken;

        public RetryingCurrentValueProvider(Func<IObservable<(T, Exception)>> observableProvider, TimeSpan retryCooldown, Action<Exception> errorCallback, HealthTracker healthTracker)
            : this(() => new RawCurrentValueProvider<T>(observableProvider, errorCallback, healthTracker), retryCooldown)
        {
            HealthTracker = healthTracker;
        }

        internal RetryingCurrentValueProvider(Func<ICurrentValueProvider<T>> currentValueProviderFactory, TimeSpan retryCooldown)
        {
            this.currentValueProviderFactory = currentValueProviderFactory;
            this.retryCooldown = retryCooldown;

            currentValueProvider = currentValueProviderFactory();
        }

        public T Get()
        {
            var currentProvider = currentValueProvider;

            try
            {
                return currentProvider.Get();
            }
            catch
            {
                ReplaceProvider(currentProvider);

                return currentValueProvider.Get();
            }
        }

        public HealthTracker HealthTracker { get; }

        public void Dispose() => currentValueProvider?.Dispose();

        private void ReplaceProvider([NotNull] ICurrentValueProvider<T> currentProvider)
        {
            if (Interlocked.CompareExchange(ref cooldownToken, new object(), null) != null)
                return;

            var newProvider = currentValueProviderFactory();
            if (Interlocked.CompareExchange(ref currentValueProvider, newProvider, currentProvider) != currentProvider)
                newProvider.Dispose();
            currentProvider.Dispose();

            Task.Delay(retryCooldown).ContinueWith(_ => Interlocked.Exchange(ref cooldownToken, null));
        }
    }
}