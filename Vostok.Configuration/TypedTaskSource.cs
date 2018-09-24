using System;
using System.Collections.Concurrent;

namespace Vostok.Configuration
{
    internal class TypedTaskSource
    {
        private readonly ConcurrentDictionary<Type, object> typedValueObservers;

        public TypedTaskSource() => typedValueObservers = new ConcurrentDictionary<Type, object>();

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