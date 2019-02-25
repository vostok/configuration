using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Helpers
{
    public static class HealingObservable
    {
        public static IObservable<(T, Exception)> PushAndResubscribeOnErrors<T>(Func<IObservable<(T, Exception)>> observe, TimeSpan cooldown)
        {
            return Observable.Defer(observe).Catch<(T, Exception), Exception>(e => Observable.Throw<(T, Exception)>(e).DelaySubscription(cooldown).StartWith((default, e))).Retry();
        }
    }
}