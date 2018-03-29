using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Vostok.Commons;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly bool throwExceptions;
        private readonly Action<Exception> callBack;
        private readonly ConcurrentBag<SourceInfo> sources;
//        private readonly Dictionary<Type, IConfigurationSource> sources;
//        private readonly ConcurrentDictionary<(Type, IConfigurationSource), CacheData> cache;
        private readonly ConcurrentBag<CacheData> cache;
        private readonly ISettingsBinder settingsBinder;
        private readonly TimeSpan cacheTime = 10.Seconds();

        private readonly List<ObserverInfo> observers;
        private readonly object sync;

        /*private class CacheData
        {
            public object Value;
            public DateTime ExpirationDate;
        }*/
        private class CacheData
        {
            public Type Type;
            public IConfigurationSource[] Sources;
            public object Value;
            public DateTime ExpirationDate;
        }
        private class SourceInfo
        {
            public Type Type;
            public IConfigurationSource Source;
        }

        /// <summary>
        /// Creating configuration provider
        /// </summary>
        public ConfigurationProvider(ISettingsBinder settingsBinder = null, bool throwExceptions = true, Action<Exception> callBack = null)
        {
            this.throwExceptions = throwExceptions;
            this.callBack = callBack;
            this.settingsBinder = settingsBinder ?? new DefaultSettingsBinder();
            sources = new ConcurrentBag<SourceInfo>();
//            sources = new Dictionary<Type, IConfigurationSource>();
//            cache = new ConcurrentDictionary<(Type, IConfigurationSource), CacheData>();
            cache = new ConcurrentBag<CacheData>();
            observers = new List<ObserverInfo>();
            sync = new object();
        }

        public TSettings Get<TSettings>()
        {
            try
            {
                if (!CanGet<TSettings>())
                    throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");
                var srcs = sources.Where(s => s.Type == typeof(TSettings)).Select(s => s.Source).ToArray();
                if (TryGetFromCache<TSettings>(srcs, out var res)) return res;
                res = settingsBinder.Bind<TSettings>(new CombinedSource(srcs).Get());
                AddToCache(typeof(TSettings), srcs, res);
                return res;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                callBack?.Invoke(e);
                return default;
            }
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            try
            {
                if (TryGetFromCache<TSettings>(source.ToEnumerable().ToArray(), out var res)) return res;
                res = settingsBinder.Bind<TSettings>(source.Get());
                AddToCache(typeof(TSettings), source.ToEnumerable().ToArray(), res);
                return res;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                callBack?.Invoke(e);
                return default;
            }
        }

        private bool TryGetFromCache<TSettings>(IConfigurationSource[] confSources, out TSettings res)
        {
            var val = cache.FirstOrDefault(c => c.Type == typeof(TSettings) && c.Sources.SequenceEqual(confSources) && c.ExpirationDate >= DateTime.UtcNow)?.Value;
            if (val != null)
            {
                res = (TSettings)val;
                return true;
            }
            res = default;
            return false;

            /*if (cache.ContainsKey((typeof(TSettings), source)))
            {
                var val = cache[(typeof(TSettings), source)];
                if (val.ExpirationDate <= DateTime.UtcNow)
                {
                    res = (TSettings)val.Value;
                    return true;
                }
            }
            res = default;
            return false;*/
        }

        private void AddToCache(Type type, IConfigurationSource[] confSources, object value)
        {
            var val = cache.FirstOrDefault(c => c.Type == type && c.Sources.SequenceEqual(confSources));
            if (val == null)
                cache.Add(new CacheData
                {
                    Value = value,
                    ExpirationDate = DateTime.UtcNow + cacheTime,
                    Type = type,
                    Sources = confSources,
                });
            else
            {
                val.Value = value;
                val.ExpirationDate = DateTime.UtcNow + cacheTime;
            }

            /*var cacheData = new CacheData
            {
                Value = value,
                ExpirationDate = DateTime.UtcNow + cacheTime,
            };
            cache.AddOrUpdate((type, source), cacheData, (tuple, data) => cacheData);*/
        }

        private bool CanGet<TSettings>() => 
            sources.Any(s => s.Type == typeof(TSettings));

        public IObservable<TSettings> Observe<TSettings>()
        {
            return Observable.Create<TSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(new ObserverInfo { Type = typeof(TSettings), Observer = observer });
                    if (CanGet<TSettings>())
                        try
                        {
                            observer.OnNext(Get<TSettings>());
                        }
                        catch (Exception e)
                        {
                            if (throwExceptions)
                                throw;
                            callBack?.Invoke(e);
                            observer.OnNext(default);
                        }
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
            {
                var obs = (IObserver<TSettings>)oi?.Observer;
                try
                {
                    obs?.OnNext(settings == null
                        ? default
                        : settingsBinder.Bind<TSettings>(settings));
                }
                catch (Exception e)
                {
                    if (throwExceptions)
                        throw;
                    callBack?.Invoke(e);
                    obs?.OnNext(default);
                }
            });

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
            if (!sources.Any(o => o.Type == typeof(TSettings) && o.Source == source))
            {
                sources.Add(new SourceInfo{ Type = typeof(TSettings), Source = source });
//                sources.Add(typeof(TSettings), source);
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

        private class ObserverInfo
        {
            public Type Type { get; set; }
            public object Observer { get; set; }
            public IConfigurationSource Source { get; set; }
        }
    }
}