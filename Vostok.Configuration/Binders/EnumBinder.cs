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
            SettingsNode.CheckSettings(settings);

            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return result;
            throw new InvalidCastException($"{nameof(EnumBinder<T>)}: value \"{settings.Value}\" for enum \"{typeof(T).Name}\" was not found.");
        }
    }
}