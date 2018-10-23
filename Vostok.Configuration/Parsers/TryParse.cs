namespace Vostok.Configuration.Parsers
{
    public delegate bool TryParse<T>(string input, out T value);
}