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
        internal class SourceInfo
        {
            public Type Type { get; set; }
            public IConfigurationSource ConfigurationSource { get; set; }
        }
        internal class ObserversInfo
        {
            public Type Type { get; set; }
            public object Observer { get; set; }
            public IConfigurationSource Source { get; set; }
        }

        private readonly List<SourceInfo> sources;

        private readonly List<ObserversInfo> observers;
        private readonly object sync;

        /// <summary>
        /// Creating configuration provider
        /// </summary>
        public ConfigurationProvider()
        {
            sources = new List<SourceInfo>();
            observers = new List<ObserversInfo>();
            sync = new object();
        }

        public TSettings Get<TSettings>()
        {
            if (!CanGet<TSettings>())
                throw new ArgumentException("IConfigurationSource for specified type is absent");

            var srcs = sources.Where(s => s.Type == typeof(TSettings)).Select(s => s.ConfigurationSource).ToArray();
            return new DefaultSettingsBinder().Bind<TSettings>(
                new CombinedSource(srcs).Get());
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            return new DefaultSettingsBinder().Bind<TSettings>(source.Get());
        }

        private bool CanGet<TSettings>() => 
            sources.Any(s => s.Type == typeof (TSettings));

        public IObservable<TSettings> Observe<TSettings>()
        {
            return Observable.Create<TSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(new ObserversInfo { Type = typeof (TSettings), Observer = observer });

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
            ObserversInfo oi = null;

            source.Observe().Subscribe(settings =>
                ((IObserver<TSettings>) oi?.Observer)?.OnNext(
                    new DefaultSettingsBinder().Bind<TSettings>(settings)));

            return Observable.Create<TSettings>(observer =>
            {
                oi = new ObserversInfo {Type = typeof (TSettings), Observer = observer, Source = source};
                return Disposable.Empty;
            });
        }

        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            if (!sources.Any(o => o.Type == typeof (TSettings) && o.ConfigurationSource == source))
            {
                sources.Add(new SourceInfo{ Type = typeof(TSettings), ConfigurationSource = source });
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