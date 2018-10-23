namespace Vostok.Configuration.Parsers
{
    internal static class StringParser
    {
        public static bool TryParse(string input, out string value)
        {
            value = input;

            return true;
        }
    }
}