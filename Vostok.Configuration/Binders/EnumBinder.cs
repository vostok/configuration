using System;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISettingsBinder<T>
        where T : struct
    {
        public SettingsBindingResult<T> Bind(ISettingsNode settings)
        {
            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return SettingsBindingResult.Success(result);

            return SettingsBindingResult.ParsingError<T>(settings.Value);
        }
    }
}