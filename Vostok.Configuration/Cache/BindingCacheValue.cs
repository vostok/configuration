using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Cache
{
    internal class BindingCacheValue<TSettings>
    {
        public BindingCacheValue(ISettingsNode lastBoundNode, TSettings settings)
        {
            LastBoundNode = lastBoundNode;
            LastSettings = settings;
        }

        public BindingCacheValue(ISettingsNode lastBoundNode, Exception error)
        {
            LastBoundNode = lastBoundNode;
            LastError = error;
        }

        public ISettingsNode LastBoundNode { get; }
        public TSettings LastSettings { get; }
        public Exception LastError { get; }
    }
}