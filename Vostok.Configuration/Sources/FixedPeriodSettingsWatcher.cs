﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Vostok.Commons.Conversions;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Watcher for settings witch not allow often access.
    /// </summary>
    public static class FixedPeriodSettingsWatcher
    {
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();
        private static readonly TimeSpan MinThreadSleepPeriod = 100.Milliseconds();
        private static readonly TimeSpan MinThreaCheckPeriod = 100.Milliseconds();

        private static bool needStop;
        private static RawSettings generalCurrent;
        private static Dictionary<TimeSpan, ObserversInfo> observers;
        private static Thread watcherThread;
        private static object sync;
        private static DateTime generalNextCheck;
        private static Func<RawSettings> generalGetMethod;
        private static TimeSpan threadSleepPeriod;
        private static TimeSpan threadCheckPeriod;

        /// <summary>
        /// Starts settings watcher
        /// </summary>
        /// <param name="sleepPeriod">Thread sleep period (min 100 ms). Can be redefined in any time.</param>
        /// <param name="checkPeriod">Thread next check period (min 100 ms). Can be redefined in any time.</param>
        /// <param name="getMethod">Get method from IConfigurationSource. Define it at least once. You can redefine it. Is method is null you just can redefine checkPeriod param.</param>
        /// <param name="observePeriod">Observation period (min 100 ms)</param>
        public static void StartFixedPeriodSettingsWatcher(TimeSpan sleepPeriod, TimeSpan checkPeriod, Func<RawSettings> getMethod = null, TimeSpan observePeriod = default)
        {
            sleepPeriod = sleepPeriod < MinThreadSleepPeriod ? MinThreadSleepPeriod : sleepPeriod;
            checkPeriod = checkPeriod < MinThreaCheckPeriod ? MinThreaCheckPeriod : checkPeriod;
            observePeriod = observePeriod < MinObservationPeriod ? MinObservationPeriod : observePeriod;
            if (observers == null)
                observers = new Dictionary<TimeSpan, ObserversInfo>();
            if (sync == null)
                sync = new object();
            if (generalGetMethod == null && getMethod == null)
                throw new ArgumentNullException("Get method is not defined. You must define it at least once.");
            else if (getMethod != null)
                lock (sync)
                    generalGetMethod = getMethod;
            lock (sync)
            {
                if (getMethod != null)
                    observers.Add(observePeriod, new ObserversInfo
                    {
                        Observers = new BehaviorSubject<RawSettings>(null),
                        ObservationPeriod = observePeriod,
                        NextCheck = DateTime.UtcNow + observePeriod,
                        Current = null,
                    });
                threadSleepPeriod = sleepPeriod;
                threadCheckPeriod = checkPeriod;
            }
            needStop = false;

            if (watcherThread == null)
            {
                generalNextCheck = DateTime.MinValue;
                threadSleepPeriod = sleepPeriod;
                watcherThread = ThreadRunner.Run(WatchSettings);
            }
        }

        /// <summary>
        /// Add subscription
        /// </summary>
        public static IObservable<RawSettings> Observe(TimeSpan observePeriod)
        {
            return Observable.Create<RawSettings>(observer =>
            {
                IDisposable subscription;
                lock (sync)
                {
                    if (observers.ContainsKey(observePeriod))
                        subscription = observers[observePeriod].Observers.Where(s => s != null).SubscribeSafe(observer);
                    else
                        return null;    //todo: null or exception?
                    if (observers[observePeriod].Current != null)
                        observer.OnNext(observers[observePeriod].Current);
                }
                return subscription;
            });
        }

        /// <summary>
        /// Stop watcher and clear all data for next start
        /// </summary>
        public static void StopAndClear()
        {
            lock (sync)
            {
                needStop = true;
                generalCurrent = null;
                foreach (var info in observers)
                    info.Value.Observers.Dispose();
                observers.Clear();
                watcherThread = null;
                generalGetMethod = null;
                sync = null;
            }
        }

        /// <summary>
        /// Remove observers of specified period
        /// </summary>
        /// <param name="observePeriod">All observers with specified period will be removed</param>
        public static void RemoveObservers(TimeSpan observePeriod)
        {
            lock (sync)
            {
                if (observers.ContainsKey(observePeriod))
                {
                    observers[observePeriod].Observers.Dispose();
                    observers.Remove(observePeriod);
                }
                if (observers.Count == 0)
                {
                    needStop = true;
                    watcherThread = null;
                }
            }
        }

        private static void WatchSettings()
        {
            while (!needStop)
            {
                Thread.Sleep(threadSleepPeriod);
                if (needStop) break;

                if (generalNextCheck <= DateTime.UtcNow)
                    lock (sync)
                    {
                        var changes = generalGetMethod();
                        if (!Equals(generalCurrent, changes))
                            generalCurrent = changes;
                        generalNextCheck = DateTime.UtcNow + threadCheckPeriod;
                    }

                lock (sync)
                {
                    var dt = DateTime.UtcNow;
                    if (observers.Count == 0 ||
                        observers.All(d => !d.Value.Observers.HasObservers) ||
                        observers.All(d => d.Value.NextCheck > dt))
                        continue;

                    foreach (var pair in observers.Where(d =>
                        d.Value.NextCheck <= DateTime.UtcNow && !Equals(d.Value.Current, generalCurrent)))
                    {
                        pair.Value.Observers.OnNext(generalCurrent);
                        pair.Value.Current = generalCurrent;
                        pair.Value.NextCheck = DateTime.UtcNow + pair.Value.ObservationPeriod;
                    }
                }
            }
            needStop = false;
        }

        private class ObserversInfo
        {
            public BehaviorSubject<RawSettings> Observers;
            public TimeSpan ObservationPeriod;
            public DateTime NextCheck;
            public RawSettings Current;
        }
    }
}