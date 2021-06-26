using System;
using System.Threading;
using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers
{
    internal class ErrorCallbackDecorator
    {
        private static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(10);

        private readonly Action<Exception> callback;
        private readonly TimeSpan cooldown;

        private volatile Tuple<Exception, DateTimeOffset> lastError;

        public ErrorCallbackDecorator([CanBeNull] Action<Exception> callback, TimeSpan? cooldown = null)
        {
            this.callback = callback;
            this.cooldown = cooldown ?? DefaultCooldown;
        }

        public void Invoke(Exception error)
        {
            try
            {
                var newLastError = Tuple.Create(error, DateTimeOffset.UtcNow);
                var oldLastError = Interlocked.Exchange(ref lastError, newLastError);
                if (oldLastError != null && ExceptionEqualityComparer.Equals(error, oldLastError.Item1) && newLastError.Item2 - oldLastError.Item2 < cooldown)
                    return;

                callback?.Invoke(error);
            }
            catch
            {
                // ignored
            }
        }
    }
}