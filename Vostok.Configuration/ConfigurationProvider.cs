using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private class ObserverInfo
        {
            public Type Type { get; set; }
            public object Observer { get; set; }
            public IConfigurationSource Source { get; set; }
        }

        private readonly Dictionary<Type, IConfigurationSource> sources;
        private readonly ISettingsBinder settingsBinder;

        private readonly List<ObserverInfo> observers;
        private readonly object sync;

        /// <summary>
        /// Creating configuration provider
        /// </summary>
        public ConfigurationProvider(ISettingsBinder settingsBinder = null)
        {
            this.settingsBinder = settingsBinder ?? new DefaultSettingsBinder();
            sources = new Dictionary<Type, IConfigurationSource>();
            observers = new List<ObserverInfo>();
            sync = new object();
        }

        public TSettings Get<TSettings>()
        {
            if (!CanGet<TSettings>())
                throw new ArgumentException($"IConfigurationSource for specified type \"{typeof(TSettings).Name}\" is absent");

            var srcs = sources.Where(s => s.Key == typeof(TSettings)).Select(s => s.Value).ToArray();
            return settingsBinder.Bind<TSettings>(new CombinedSource(srcs).Get());
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            return settingsBinder.Bind<TSettings>(source.Get());
        }

        private bool CanGet<TSettings>() => 
            sources.Any(s => s.Key == typeof(TSettings));

        public IObservable<TSettings> Observe<TSettings>()
        {
            return Observable.Create<TSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(new ObserverInfo { Type = typeof(TSettings), Observer = observer });

                    if (CanGet<TSettings>())
                        observer.OnNext(Get<TSettings>());
                }
                return Disposable.Create(() =>
                {
                    lock (sync)
                    {
                        observers.RemoveAll(o => o.Type == typeof(TSettings) && o.Observer == observer && o.Source == null);
                    }
                });
            });
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            ObserverInfo oi = null;

            var sub = source.Observe().Subscribe(settings =>
                ((IObserver<TSettings>)oi?.Observer)?.OnNext(
                    settings == null ? default : settingsBinder.Bind<TSettings>(settings)));

            return Observable.Create<TSettings>(observer =>
            {
                oi = new ObserverInfo {Type = typeof(TSettings), Observer = observer, Source = source};
                return Disposable.Create(() =>
                {
                    sub.Dispose();
                    oi = null;
                });
            });
        }

        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            if (!sources.Any(o => o.Key == typeof(TSettings) && o.Value == source))
            {
                sources.Add(typeof(TSettings), source);
                source.Observe().Subscribe(settings =>
                {
                    if (CanGet<TSettings>())
                        lock (sync)
                        {
                            var res = Get<TSettings>();
                            foreach (var obsInfo in observers.Where(o => o.Type == typeof(TSettings) && o.Source == null))
                                ((IObserver<TSettings>) obsInfo.Observer).OnNext(res);
                        }
                });
            }
            return this;
        }
    }
}