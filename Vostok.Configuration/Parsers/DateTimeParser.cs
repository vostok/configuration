using System;

namespace Vostok.Configuration.Parsers
{
    internal static class DateTimeParser
    {
        public static bool TryParse(string input, out DateTime result)
        {
            var res = DateTimeOffsetParser.TryParse(input, out var dt);
            result = res ? dt.UtcDateTime : default;
            return res;
        }

        public static DateTime Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;
            throw new FormatException($"{nameof(DateTimeParser)}. Failed to parse from string '{input}'.");
        }
    }
}