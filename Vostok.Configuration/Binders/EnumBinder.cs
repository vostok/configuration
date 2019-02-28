using System;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISafeSettingsBinder<T>
        where T : struct
    {
        public SettingsBindingResult<T> Bind(ISettingsNode settings)
        {
            if (settings.IsNullOrMissing())
                return SettingsBindingResult.Success(default(T));

            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return SettingsBindingResult.Success(result);

            return SettingsBindingResult.ParsingError<T>(settings.Value);
        }
    }
}