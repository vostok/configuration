using System;

namespace Vostok.Configuration.Parsers
{
    internal static class UriParser
    {
        public static bool TryParse(string input, out Uri result) =>
            Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out result);

        public static Uri Parse(string input)
        {
            if (TryParse(input, out var result))
                return result;
            throw new FormatException($"{nameof(UriParser)}. Failed to parse from string '{input}'.");
        }
    }
}