using System;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IBindingCacheItem<TSettings>, IDisposable
    {
        private readonly AtomicBoolean isDisposed = new AtomicBoolean(false);

        private ICurrentValueProvider<TSettings> currentValueProvider;

        public (TSettings settings, Exception error)? LastValue { get; set; }

        public BindingCacheValue<TSettings> BindingCacheValue { get; set; }

        public ICurrentValueProvider<TSettings> CurrentValueProvider => currentValueProvider;

        public bool IsDisposed => isDisposed;

        public bool TrySetCurrentValueProvider(ICurrentValueProvider<TSettings> currentValueProvider)
        {
            return Interlocked.CompareExchange(ref this.currentValueProvider, currentValueProvider, null) == null && !IsDisposed;
        }

        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
                currentValueProvider?.Dispose();
        }
    }
}
