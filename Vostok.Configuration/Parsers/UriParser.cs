using System;

namespace Vostok.Configuration.Parsers
{
    internal static class UriParser
    {
        public static bool TryParse(string input, out Uri result) =>
            Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out result);
    }
}