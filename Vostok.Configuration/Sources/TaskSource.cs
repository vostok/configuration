using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    // CR(krait): This class seems to have two completely separate responsibilities:
    // CR(krait): 1. Provide last value caching for configuration sources.
    // CR(krait): 2. Provide last value caching for configuration provider.
    // CR(krait): Let's split it accordingly.
    internal class TaskSource
    {
        // CR(krait): There's a lot of duplicate logic for typed and ISettingsNode sources. A simple snippet of how it could be improved: https://gist.github.com/DQKrait/4597b4955d5c450249116a1e42e1d6ad
        private readonly ConcurrentDictionary<Type, TaskCompletionSource<object>> typeResultSources;
        private readonly ConcurrentDictionary<Type, object> typeLastValues;
        private readonly ConcurrentDictionary<Type, IDisposable> typeInnerSubscriptions;

        private volatile TaskCompletionSource<ISettingsNode> resultSource;
        private ISettingsNode lastValue;
        private IDisposable innerSubscription;

        public TaskSource()
        {
            resultSource = new TaskCompletionSource<ISettingsNode>();
            lastValue = null;
            typeResultSources = new ConcurrentDictionary<Type, TaskCompletionSource<object>>();
            typeLastValues = new ConcurrentDictionary<Type, object>();
            typeInnerSubscriptions = new ConcurrentDictionary<Type, IDisposable>();
        }

        // CR(krait): Get() should never throw exceptions. If the source is unavailable, just return null.
        public ISettingsNode Get(IObservable<ISettingsNode> observable)
        {
            // CR(krait): This is not thread-safe. You may subscribe multiple times and not dispose the extra subscriptions.
            if (innerSubscription == null)
                innerSubscription = observable.Subscribe(ResultOnNext, ResultOnError);
            return resultSource.Task.GetAwaiter().GetResult();
        }

        public T Get<T>(IObservable<T> observable)
        {
            var type = typeof(T);
            if (!typeResultSources.TryGetValue(type, out var source))
            {
                source = new TaskCompletionSource<object>();
                typeResultSources.TryAdd(type, source);
            }

            // CR(krait): This is not thread-safe. You may subscribe multiple times and not dispose the extra subscriptions.
            if (!typeInnerSubscriptions.ContainsKey(type))
                typeInnerSubscriptions[type] = observable.Subscribe(ResultOnNext, ResultOnError<T>);

            return (T)source.Task.GetAwaiter().GetResult();
        }

        private void ResultOnNext(ISettingsNode settings)
        {
            lastValue = settings;
            if (!resultSource.Task.IsCompleted)
                resultSource.TrySetResult(settings);
            else
                resultSource = NewCompletedSource(settings);
        }

        private void ResultOnNext<T>(T settings)
        {
            var type = typeof(T);
            typeLastValues[type] = settings;
            if (!resultSource.Task.IsCompleted)
                typeResultSources[type].TrySetResult(settings);
            else
                typeResultSources[type] = NewObjectCompletedSource(settings);
        }

        private void ResultOnError(Exception exception)
        {
            if (!resultSource.Task.IsCompleted)
                resultSource.TrySetException(exception);
            else
                resultSource = NewCompletedSource(lastValue); // CR(krait): We should return null in configuration sources when configuration becomes unavailable.
        }

        private void ResultOnError<T>(Exception exception)
        {
            var type = typeof(T);
            if (!typeResultSources[type].Task.IsCompleted)
                typeResultSources[type].TrySetException(exception);
            else
                typeResultSources[type] = NewObjectCompletedSource(typeLastValues[type]);
        }

        private static TaskCompletionSource<ISettingsNode> NewCompletedSource(ISettingsNode value)
        {
            var newSource = new TaskCompletionSource<ISettingsNode>();
            newSource.TrySetResult(value);
            return newSource;
        }

        private static TaskCompletionSource<object> NewObjectCompletedSource(object value)
        {
            var newSource = new TaskCompletionSource<object>();
            newSource.TrySetResult(value);
            return newSource;
        }
    }
}