using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Helper
{
    internal class SingleFileWatcherSubstitute : IObservable<string>
    {
        private Subject<string> observers;
        private string currentValue;
        private readonly object locker;
        private bool initialized;

        public SingleFileWatcherSubstitute([NotNull] string filePath, FileSourceSettings encoding)
        {
            observers = new Subject<string>();
            currentValue = null;
            initialized = false;
            locker = new object();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (observers.IsDisposed)
                observers = new Subject<string>();
            observers.Subscribe(observer);
            lock (locker)
                if (initialized)
                    observer.OnNext(currentValue);

            return observers;
        }

        /// <summary>
        /// Imitates file creating/updating. Do not send OnNext if new and old values are equal.
        /// </summary>
        /// <param name="newValue">File content</param>
        /// <param name="ignoreIfEquals">Ignore if old and new values are equal. Always send OnNext for observers</param>
        public void GetUpdate(string newValue, bool ignoreIfEquals = false)
        {
            initialized = true;
            var isNew = newValue != currentValue;
            currentValue = newValue;
            if (isNew || ignoreIfEquals)
                observers.OnNext(currentValue);
        }

        /// <summary>
        /// Imitates throwing exeptions on reading file
        /// </summary>
        /// <param name="e">Some exception</param>
        public void ThrowException(Exception e) => observers.OnError(e);
    }
}