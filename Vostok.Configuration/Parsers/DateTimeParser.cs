using System;

namespace Vostok.Configuration.Parsers
{
    internal static class DateTimeParser
    {
        public static bool TryParse(string input, out DateTime result)
        {
            var res = DateTimeOffsetParser.TryParse(input, out var dt);
            result = res ? dt.DateTime : default;
            return res;
        }
    }
}