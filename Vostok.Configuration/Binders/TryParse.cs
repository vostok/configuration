namespace Vostok.Configuration.Binders
{
    public delegate bool TryParse<T>(string s, out T value);
}