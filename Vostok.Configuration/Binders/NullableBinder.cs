﻿using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class NullableBinder<T> : ISettingsBinder<T?>
        where T : struct
    {
        private readonly ISettingsBinder<T> elementBinder;

        public NullableBinder(ISettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public T? Bind(ISettingsNode settings)
        {
            SettingsNode.CheckSettings(settings);
            return settings.Value == null ? (T?)null : elementBinder.Bind(settings);
        }
    }
}