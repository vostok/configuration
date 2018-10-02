namespace Vostok.Configuration.Parsers
{
    internal class StringParser : ITypeParser
    {
        public bool TryParse(string s, out object value)
        {
            value = s;

            return true;
        }
    }
}