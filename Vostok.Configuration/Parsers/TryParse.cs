using JetBrains.Annotations;

namespace Vostok.Configuration.Parsers
{
    [PublicAPI]
    public delegate bool TryParse<T>(string input, out T value);
}