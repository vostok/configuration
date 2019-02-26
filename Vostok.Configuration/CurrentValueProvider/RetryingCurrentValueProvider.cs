using System;
using System.Threading;
using JetBrains.Annotations;

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
            var newProvider = currentValueProviderFactory();
            if (Interlocked.CompareExchange(ref currentValueProvider, newProvider, currentProvider) != currentProvider)
                newProvider.Dispose();
            currentProvider.Dispose();
        }
    }
}