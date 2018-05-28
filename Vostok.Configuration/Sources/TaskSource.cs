using System;
using System.Threading.Tasks;

namespace Vostok.Configuration.Sources
{
    internal class TaskSource
    {
        private volatile TaskCompletionSource<IRawSettings> resultSource;
        private IRawSettings lastValue;
        private IDisposable innerSubscription;

        public TaskSource()
        {
            resultSource = new TaskCompletionSource<IRawSettings>();
            lastValue = null;
        }

        public IRawSettings Get(IObservable<IRawSettings> observable)
        {
            if (innerSubscription == null)
                innerSubscription = observable.Subscribe(ResultOnNext, ResultOnError);
            return resultSource.Task.GetAwaiter().GetResult();
        }

        private void ResultOnNext(IRawSettings settings)
        {
            lastValue = settings;
            if (!resultSource.Task.IsCompleted)
                resultSource.TrySetResult(settings);
            else
                resultSource = NewCompletedSource(settings);
        }

        private void ResultOnError(Exception exception)
        {
            if (!resultSource.Task.IsCompleted)
                resultSource.TrySetException(exception);
            else
                resultSource = NewCompletedSource(lastValue);
        }

        private static TaskCompletionSource<IRawSettings> NewCompletedSource(IRawSettings value)
        {
            var newSource = new TaskCompletionSource<IRawSettings>();
            newSource.TrySetResult(value);
            return newSource;
        }
    }
}