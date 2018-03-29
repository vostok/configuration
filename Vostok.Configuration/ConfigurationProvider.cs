using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Vostok.Commons.Conversions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly bool throwExceptions;
        private readonly Action<Exception> onError;
        private readonly Dictionary<Type, IConfigurationSource> sources;
        private readonly ISettingsBinder settingsBinder;
        private readonly TimeSpan cacheTime;

        private readonly ConcurrentDictionary<Type, (object value, DateTime expiration)> cache;
        private readonly ConcurrentDictionary<Type, BehaviorSubject<object>> subjects;

        /// <summary>
        /// Creating configuration provider
        /// </summary>
        public ConfigurationProvider(ISettingsBinder settingsBinder = null, bool throwExceptions = true, Action<Exception> onError = null, TimeSpan cacheTime = default)
        {
            this.throwExceptions = throwExceptions;
            this.onError = onError;
            this.cacheTime = cacheTime != default ? cacheTime : 10.Seconds();
            this.settingsBinder = settingsBinder ?? new DefaultSettingsBinder();
            sources = new Dictionary<Type, IConfigurationSource>();
            cache = new ConcurrentDictionary<Type, (object, DateTime)>();
            subjects = new ConcurrentDictionary<Type, BehaviorSubject<object>>();
        }

        public TSettings Get<TSettings>()
        {
            try
            {
                if (cache.TryGetValue(typeof (TSettings), out var item) && DateTime.UtcNow < item.expiration)
                    return (TSettings)item.value;
                
                if (!sources.TryGetValue(typeof(TSettings), out var source))
                    throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");

                var value = settingsBinder.Bind<TSettings>(source.Get());

                cache[typeof(TSettings)] = (value, DateTime.UtcNow + cacheTime);

                return value;
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return default;
            }
        }

        public TSettings Get<TSettings>(IConfigurationSource source)
        {
            try
            {
                return settingsBinder.Bind<TSettings>(source.Get());
            }
            catch (Exception e)
            {
                if (throwExceptions)
                    throw;
                onError?.Invoke(e);
                return default;
            }
        }

        public IObservable<TSettings> Observe<TSettings>()
        {
            if (!sources.TryGetValue(typeof(TSettings), out var source))
                throw new ArgumentException($"{nameof(IConfigurationSource)} for specified type \"{typeof(TSettings).Name}\" is absent");

            var newSubject = null as BehaviorSubject<object>;
            var subject = subjects.GetOrAdd(typeof(TSettings), _ => newSubject = new BehaviorSubject<object>(Get<TSettings>()));

            if (subject == newSubject)
                source.Observe().Where(s => s != null).Select(_ => Get<TSettings>() as object).Subscribe(subject);

            return subject.Where(s => s != null).Cast<TSettings>();
        }

        public IObservable<TSettings> Observe<TSettings>(IConfigurationSource source)
        {
            return source.Observe().Select(settings => settingsBinder.Bind<TSettings>(settings)).Where(s => s != null);
        }

        public ConfigurationProvider WithSourceFor<TSettings>(IConfigurationSource source)
        {
            if (subjects.Any())
                throw new InvalidOperationException($"It is not allowed to add sources to a {nameof(ConfigurationProvider)} after .{nameof(Observe)}() was called.");

            if (sources.TryGetValue(typeof(TSettings), out var existingSource))
                sources[typeof(TSettings)] = existingSource.Combine(source);
            else
                sources[typeof(TSettings)] = source;

            return this;
        }
    }
}