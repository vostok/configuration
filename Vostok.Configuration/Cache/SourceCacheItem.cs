using System;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IBindingCacheItem<TSettings>, IDisposable
    {
        private readonly object currentValueProviderSync = new object();

        /// <summary>
        /// <para>A pair of settings node and the result of its binding.</para>
        /// <para>Used for avoiding repeated binds of same node for every observer.</para>
        /// </summary>
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