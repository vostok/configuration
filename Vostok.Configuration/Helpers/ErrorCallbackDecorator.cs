using System;
using System.Threading;
using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers
{
    internal class ErrorCallbackDecorator
    {
        private readonly Action<Exception> callback;
        private Exception lastError;

        public ErrorCallbackDecorator([CanBeNull] Action<Exception> callback)
        {
            this.callback = callback;
        }

        public void Invoke(Exception error)
        {
            try
            {
                if (ExceptionEqualityComparer.Equals(error, Interlocked.Exchange(ref lastError, error)))
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