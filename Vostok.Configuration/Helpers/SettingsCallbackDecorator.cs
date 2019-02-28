using System;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Helpers
{
    internal class SettingsCallbackDecorator
    {
        private readonly Action<object, IConfigurationSource> callback;
        private readonly Action<Exception> errorCallback;

        private Tuple<object, IConfigurationSource> lastValue;

        public SettingsCallbackDecorator(
            [CanBeNull] Action<object, IConfigurationSource> callback,
            [CanBeNull] Action<Exception> errorCallback)
        {
            this.callback = callback;
            this.errorCallback = errorCallback;
        }

        public void Invoke(object settings, IConfigurationSource source)
        {
            try
            {
                var currentValue = Tuple.Create(settings, source);
                var previousValue = Interlocked.Exchange(ref lastValue, currentValue);

                if (ReferenceEquals(currentValue.Item1, previousValue?.Item1) &&
                    ReferenceEquals(currentValue.Item2, previousValue?.Item2))
                {
                    return;
                }

                callback?.Invoke(settings, source);
            }
            catch (Exception error)
            {
                errorCallback?.Invoke(error);
            }
        }
    }
}