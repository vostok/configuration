using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISettingsBinder<T>
        where T : struct
    {
        public T Bind(ISettingsNode settings)
        {
            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return result;

            throw new SettingsBindingException($"Value '{settings.Value}' is not valid for enum of type '{typeof(T)}'.");
        }
    }
}