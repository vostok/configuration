using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Cache
{
    internal class BindingCacheValue<TSettings>
    {

        public BindingCacheValue(ISettingsBinder usedBinder, ISettingsNode lastBoundNode, TSettings settings)
        {
            UsedBinder = usedBinder;
            LastBoundNode = lastBoundNode;
            LastSettings = settings;
        }

        public BindingCacheValue(ISettingsBinder usedBinder, ISettingsNode lastBoundNode, Exception error)
        {
            UsedBinder = usedBinder;
            LastBoundNode = lastBoundNode;
            LastError = error;
        }

        public ISettingsBinder UsedBinder { get; }
        public ISettingsNode LastBoundNode { get; }
        public TSettings LastSettings { get; }
        public Exception LastError { get; }
    }
}