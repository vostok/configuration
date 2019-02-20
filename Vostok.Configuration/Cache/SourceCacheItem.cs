using System;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IBindingCacheItem<TSettings>, IDisposable
    {
        private readonly object currentValueProviderSync = new object();

        public (TSettings settings, Exception error)? LastValue { get; set; }

        public BindingCacheValue<TSettings> BindingCacheValue { get; set; }

        public ICurrentValueProvider<TSettings> CurrentValueProvider { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool TrySetCurrentValueProvider(ICurrentValueProvider<TSettings> currentValueProvider)
        {
            if (IsDisposed || CurrentValueProvider != null)
                return false;

            lock (currentValueProviderSync)
            {
                if (IsDisposed || CurrentValueProvider != null)
                    return false;

                CurrentValueProvider = currentValueProvider;
                return true;
            }
        }

        public void Dispose()
        {
            lock (currentValueProviderSync)
            {
                IsDisposed = true;
                CurrentValueProvider?.Dispose();
            }
        }
    }
}