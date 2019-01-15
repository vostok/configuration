using System;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IBindingCacheItem<TSettings>, IDisposable
    {
        private readonly AtomicBoolean isDisposed = new AtomicBoolean(false);

        private ITaskSource<TSettings> taskSource;

        public (TSettings settings, Exception error)? LastValue { get; set; }

        public BindingCacheValue<TSettings> BindingCacheValue { get; set; }

        public ITaskSource<TSettings> TaskSource => taskSource;

        public bool IsDisposed => isDisposed;

        public bool TrySetTaskSource(ITaskSource<TSettings> source)
        {
            return Interlocked.CompareExchange(ref taskSource, source, null) == null && !IsDisposed;
        }

        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
                taskSource?.Dispose();
        }
    }
}
