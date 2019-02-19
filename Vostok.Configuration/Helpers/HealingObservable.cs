using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Helpers
{
    internal static class HealingObservable
    {
        public static IObservable<(T, Exception)> PushErrors<T>(Func<IObservable<(T, Exception)>> observe, TimeSpan cooldown)
        {
            return observe()
                .Catch<(T, Exception), Exception>(e => Observable.Defer(() => PushErrors(observe, cooldown)).Delay(cooldown).StartWith((default, e)));
        }
    }
}