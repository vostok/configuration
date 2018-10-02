namespace Vostok.Configuration.Parsers
{
    public delegate bool TryParse<T>(string s, out T value);
}