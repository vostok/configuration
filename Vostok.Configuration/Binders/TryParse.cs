using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Binders
{
    /// <summary>
    /// Delegate for adding own parser into <see cref="ISettingsBinder"/>
    /// </summary>
    /// <typeparam name="T">Type to parse in</typeparam>
    /// <param name="s">SOurce string</param>
    /// <param name="value">Result value</param>
    public delegate bool TryParse<T>(string s, out T value);
}