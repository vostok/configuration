using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    // CR(krait): This class still contains stuff for both ConfigSource and ConfigProvider. Let's fix it.
    internal class TaskSource
    {
        private readonly ConcurrentDictionary<Type, object> typedValueObservers;

        private CurrentValueObserver<ISettingsNode> rawValueObserver;

        public TaskSource()
        {
            rawValueObserver = new CurrentValueObserver<ISettingsNode>();
            typedValueObservers = new ConcurrentDictionary<Type, object>();
        }

        public ISettingsNode Get(IObservable<ISettingsNode> observable)
        {
            try
            {
                return rawValueObserver.Get(observable);
            }
            catch
            {
                rawValueObserver = new CurrentValueObserver<ISettingsNode>();
                throw;
            }
        }

        public T Get<T>(IObservable<T> observable)
        {
            var observer = (CurrentValueObserver<T>)typedValueObservers.GetOrAdd(typeof(T), _ => new CurrentValueObserver<T>());

            try
            {
                return observer.Get(observable);
            }
            catch
            {
                typedValueObservers.TryRemove(typeof(T), out _);
                throw;
            }
        }
    }
}