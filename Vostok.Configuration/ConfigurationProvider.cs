using System;
using System.Collections.Generic;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    // TODO(krait): Implement the default configuration provider.
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly List<IConfigurationSource> sources;

//        private readonly List<IObserver<object>> observers;
//        private readonly object sync;

        public ConfigurationProvider()
        {
            sources = new List<IConfigurationSource>();
        }

        public TSettings Get<TSettings>()
        {
            return new DefaultSettingsBinder().Bind<TSettings>(
                new CombinedSource(sources.ToArray()).Get());
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            return new DefaultSettingsBinder().Bind<TSettings>(source.Get());
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            /*return Observable.Create<TSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add((IObserver<object>) observer);
                    observer.OnNext(Get<TSettings>());
                }
                return Disposable.Create(() =>
                {
                    lock (sync)
                    {
                        observers.Remove((IObserver<object>) observer);
                    }
                });
            });*/
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            throw new NotImplementedException();
        }

        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            sources.Add(source);
            return this;
        }
    }
}