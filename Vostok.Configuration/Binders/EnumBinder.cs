using System;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISettingsBinder<T>
        where T : struct
    {
        public T Bind(IRawSettings settings)
        {
            RawSettings.CheckSettings(settings);

            if (Enum.TryParse<T>(settings.Value, true, out var result))
                return result;
            throw new InvalidCastException($"{nameof(EnumBinder<T>)}: value \"{settings.Value}\" for enum \"{typeof(T).Name}\" was not found.");
        }
    }
}