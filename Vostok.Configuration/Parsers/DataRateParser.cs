using System;
using Vostok.Configuration.Primitives;

namespace Vostok.Configuration.Parsers
{
    internal static class DataRateParser
    {
        private const string Second1 = "/s";
        private const string Second2 = "/sec";
        private const string Second3 = "/second";

        public static bool TryParse(string input, out DataRate result)
        {
            input = input
                .ToLower()
                .Replace(Second3, string.Empty)
                .Replace(Second2, string.Empty)
                .Replace(Second1, string.Empty)
                .Trim('.')
                .Trim();

            if (DataSizeParser.TryParse(input, out var res))
            {
                result = res / TimeSpan.FromSeconds(1);
                return true;
            }

            result = default;
            return false;
        }

        public static DataRate Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;

            throw new FormatException($"{nameof(DataRateParser)}: error in parsing string '{input}' to {nameof(DataRate)}.");
        }
    }
}