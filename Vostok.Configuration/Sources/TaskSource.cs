using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    internal class TaskSource
    {
        private CurrentValueObserver<ISettingsNode> rawValueObserver;

        public TaskSource() => rawValueObserver = new CurrentValueObserver<ISettingsNode>();

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
    }
}