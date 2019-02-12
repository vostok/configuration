using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Helpers
{
    internal static class HealingObservable
    {
        public static IObservable<T> Create<T>(Func<IObservable<T>> observe, TimeSpan cooldown)
        {
            return observe().Catch<T, Exception>(_ => Observable.Defer(() => Create(observe, cooldown)).Delay(cooldown));
        }
    }
}