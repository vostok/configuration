using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISettingsBinder<T>
        where T : struct
    {
        public T Bind(ISettingsNode settings)
        {
            if (settings == null)
                return default;
            
            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return result;

            throw new SettingsBindingException($"Value '{settings.Value}' is not valid for enum of type '{typeof(T)}'.");
        }
    }
}