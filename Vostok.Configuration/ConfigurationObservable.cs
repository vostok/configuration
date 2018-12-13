using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration
{
    internal class ConfigurationObservable : IConfigurationObservable
    {
        private readonly IConfigurationWithErrorsObservable observable;
        private readonly Action<Exception> errorCallBack;

        public ConfigurationObservable(IConfigurationWithErrorsObservable observable, Action<Exception> errorCallBack = null)
        {
            this.observable = observable;
            this.errorCallBack = errorCallBack ?? (_ => {});
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            return observable.ObserveWithErrors<TSettings>().SendErrorsToCallback(errorCallBack);
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            return observable.ObserveWithErrors<TSettings>(source).SendErrorsToCallback(errorCallBack);
        }
    }
}