﻿using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class SafeBinderWrapper<T> : ISafeSettingsBinder<T>
    {
        public ISettingsBinder<T> Binder { get; }

        public SafeBinderWrapper(ISettingsBinder<T> binder) => this.Binder = binder;

        public SettingsBindingResult<T> Bind(ISettingsNode rawSettings)
        {
            try
            {
                return SettingsBindingResult.Success(Binder.Bind(rawSettings));
            }
            catch (Exception error)
            {
                return SettingsBindingResult.Error<T>(error.ToString());
            }
        }
    }
}