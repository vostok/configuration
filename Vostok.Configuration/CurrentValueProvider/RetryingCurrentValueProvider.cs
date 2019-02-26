using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Func<ICurrentValueProvider<T>> currentValueProviderFactory;
        private readonly TimeSpan retryCooldown;

        private ICurrentValueProvider<T> currentValueProvider;
        private object cooldownToken;

        public RetryingCurrentValueProvider(Func<IObservable<T>> observableProvider, TimeSpan retryCooldown)
            : this(() => new RawCurrentValueProvider<T>(observableProvider), retryCooldown)
        {
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