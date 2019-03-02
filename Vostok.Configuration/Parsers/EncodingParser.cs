using System.Text;

namespace Vostok.Configuration.Parsers
{
    internal static class EncodingParser
    {
        public static bool TryParse(string input, out Encoding value)
        {
            try
            {
                value = Encoding.GetEncoding(input);
            }
            catch
            {
                value = null;
            }

            return value != null;
        }
    }
}