using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.ProviderComponents
{
    internal class ConfigurationObservable : IConfigurationObservable
    {
        private readonly IConfigurationWithErrorsObservable observable;
        private readonly Action<Exception> errorCallback;

        public ConfigurationObservable(IConfigurationWithErrorsObservable observable, Action<Exception> errorCallback = null)
        {
            this.observable = observable;
            this.errorCallback = errorCallback ?? (_ => {});
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            return observable.ObserveWithErrors<TSettings>().SendErrorsToCallback(errorCallback);
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            return observable.ObserveWithErrors<TSettings>(source).SendErrorsToCallback(errorCallback);
        }
    }
}