namespace Vostok.Configuration.Parsers
{
    internal interface ITypeParser
    {
        bool TryParse(string s, out object value);
    }
}