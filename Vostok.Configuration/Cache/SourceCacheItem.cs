using System;
using System.Threading;
using Vostok.Commons.Threading;
using Vostok.Configuration.Binders;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Cache
{
    internal class SourceCacheItem<TSettings> : IDisposable
    {
        public ValueWrapper<(TSettings settings, Exception error)> LastValue { get; set; }
        public CachingBinder.BindingCacheItem<TSettings> BindingCacheItem { get; } = new CachingBinder.BindingCacheItem<TSettings>();
        public ITaskSource<TSettings> TaskSource => taskSource;
        public bool IsDisposed => isDisposed;
        
        private ITaskSource<TSettings> taskSource;
        private readonly AtomicBoolean isDisposed = new AtomicBoolean(false); 

        public bool TrySetTaskSource(ITaskSource<TSettings> taskSource)
        {
            return Interlocked.CompareExchange(ref this.taskSource, taskSource, null) == null;
        }

        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
                taskSource?.Dispose();
        }
    }
}