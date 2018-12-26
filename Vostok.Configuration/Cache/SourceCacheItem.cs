using System;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IBindingCacheItem<TSettings>, IDisposable
    {
        public (TSettings settings, Exception error)? LastValue { get; set; }

        public BindingCacheValue<TSettings> BindingCacheValue { get; set; }
        
        public ITaskSource<TSettings> TaskSource => taskSource;

        public bool IsDisposed => isDisposed;
        
        private ITaskSource<TSettings> taskSource;
        
        private readonly AtomicBoolean isDisposed = new AtomicBoolean(false);

        public bool TrySetTaskSource(ITaskSource<TSettings> taskSource)
        {
            return Interlocked.CompareExchange(ref this.taskSource, taskSource, null) == null && !IsDisposed;
        }

        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
                taskSource?.Dispose();
        }
    }
}