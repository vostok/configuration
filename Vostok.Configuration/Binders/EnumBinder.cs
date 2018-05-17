using System;

namespace Vostok.Configuration.Binders
{
    internal class EnumBinder<T> : ISettingsBinder<T>
        where T : struct
    {
        public T Bind(RawSettings rawSettings)
        {
            if (Enum.TryParse<T>(rawSettings.Value, true, out var result))
                return result;
            throw new InvalidCastException($"Value \"{rawSettings.Value}\" for enum \"{typeof(T).Name}\" was not found.");
        }
    }
}