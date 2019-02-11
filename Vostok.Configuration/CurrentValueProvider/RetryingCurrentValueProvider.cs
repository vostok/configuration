using System;
using System.Threading;

namespace Vostok.Configuration.CurrentValueProvider
{
    internal class RetryingCurrentValueProvider<T> : ICurrentValueProvider<T>
    {
        private readonly Func<ICurrentValueProvider<T>> currentValueProviderFactory;
        private ICurrentValueProvider<T> currentValueProvider;

        public RetryingCurrentValueProvider(Func<IObservable<T>> observableProvider)
            : this(() => new RawCurrentValueProvider<T>(observableProvider))
        {
        }

        internal RetryingCurrentValueProvider(Func<ICurrentValueProvider<T>> currentValueProviderFactory)
        {
            this.currentValueProviderFactory = currentValueProviderFactory;
            currentValueProvider = currentValueProviderFactory();
        }

        public T Get()
        {
            try
            {
                return currentValueProvider.Get();
            }
            catch
            {
                ReplaceProvider();
                return currentValueProvider.Get();
            }
        }

        public void Dispose() => currentValueProvider?.Dispose();

        private void ReplaceProvider()
        {
            var currentProvider = currentValueProvider;
            var newProvider = currentValueProviderFactory();
            if (Interlocked.CompareExchange(ref currentValueProvider, newProvider, currentProvider) != currentProvider)
                newProvider.Dispose();
            currentProvider?.Dispose();
        }
    }
}