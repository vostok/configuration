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
        // CR(krait): Why not private?
        internal class SourceInfo
        {
            public Type Type { get; set; }
            public IConfigurationSource ConfigurationSource { get; set; }
        }
        internal class ObserverInfo
        {
            public Type Type { get; set; }
            public object Observer { get; set; }
            public IConfigurationSource Source { get; set; }
        }

        private readonly List<SourceInfo> sources;

        private readonly List<ObserverInfo> observers;
        private readonly object sync;

        /// <summary>
        /// Creating configuration provider
        /// </summary>
        public ConfigurationProvider()
        {
            sources = new List<SourceInfo>();
            observers = new List<ObserverInfo>();
            sync = new object();
        }

        public TSettings Get<TSettings>()
        {
            if (!CanGet<TSettings>())
                throw new ArgumentException("IConfigurationSource for specified type is absent"); // CR(krait): Which type?

            var srcs = sources.Where(s => s.Type == typeof(TSettings)).Select(s => s.ConfigurationSource).ToArray();
            return new DefaultSettingsBinder().Bind<TSettings>(
                new CombinedSource(srcs).Get());
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            // CR(krait): User must be able to pass their own binder through a constructor.
            return new DefaultSettingsBinder().Bind<TSettings>(source.Get());
        }

        private bool CanGet<TSettings>() => 
            sources.Any(s => s.Type == typeof(TSettings));

        public IObservable<TSettings> Observe<TSettings>()
        {
            return Observable.Create<TSettings>(observer =>
            {
                // CR(krait): Why store observers? Could do something like this:
                // CR(krait): sources[...].Observe().Subscribe(_ => observer.OnNext(Get<TSettings>()))
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

            // CR(krait): Will it work if someone calls .Subscribe() twice on the returned observable?
            source.Observe().Subscribe(settings =>
                ((IObserver<TSettings>) oi?.Observer)?.OnNext(
                    new DefaultSettingsBinder().Bind<TSettings>(settings)));

            return Observable.Create<TSettings>(observer =>
            {
                oi = new ObserverInfo {Type = typeof(TSettings), Observer = observer, Source = source};
                return Disposable.Empty;
            });
        }

        // CR(krait): There is at most one source for a type. Let's just store sources in a dictionary by type.
        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            if (!sources.Any(o => o.Type == typeof(TSettings) && o.ConfigurationSource == source))
            {
                sources.Add(new SourceInfo { Type = typeof(TSettings), ConfigurationSource = source });
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