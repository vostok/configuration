using System;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Sources
{
    internal class TaskSource
    {
        private CurrentValueObserver<(ISettingsNode, Exception)> rawValueObserver;

        public TaskSource() => rawValueObserver = new CurrentValueObserver<(ISettingsNode, Exception)>();

        public (ISettingsNode settings, Exception error) Get(IObservable<(ISettingsNode, Exception)> observable)
        {
            try
            {
                return rawValueObserver.Get(observable);
            }
            catch
            {
                rawValueObserver = new CurrentValueObserver<(ISettingsNode, Exception)>();
                throw;
            }
        }
    }
}