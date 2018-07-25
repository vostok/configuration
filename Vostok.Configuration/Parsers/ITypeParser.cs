namespace Vostok.Configuration.Parsers
{
    public interface ITypeParser
    {
        bool TryParse(string s, out object value);
    }
}