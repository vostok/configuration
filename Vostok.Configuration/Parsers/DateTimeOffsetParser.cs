using System;
using System.Globalization;

namespace Vostok.Configuration.Parsers
{
    internal static class DateTimeOffsetParser
    {
        private static CultureInfo usCulture;
        private static CultureInfo ruCulture;

        public static bool TryParse(string input, out DateTimeOffset result)
        {
            if (usCulture == null)
                usCulture = CultureInfo.GetCultureInfo("en-US");
            if (ruCulture == null)
                ruCulture = CultureInfo.GetCultureInfo("ru-RU");

            return DateTimeOffset.TryParse(input, out result)
                   || DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParse(input, ruCulture, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParse(input, usCulture, DateTimeStyles.AllowWhiteSpaces, out result)
                   || TryParseDatetimeOffset(input, ".", out result)
                   || TryParseDatetimeOffset(input, "/", out result)
                   || TryParseDatetimeOffset(input, "-", out result)
                   || DateTimeOffset.TryParseExact(input, "yyyyMMddTHHmmsszzz", null, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParseExact(input, "yyyyMMddTHHmmss", null, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParseExact(input, "yyyyMMdd", null, DateTimeStyles.AllowWhiteSpaces, out result)
                   || DateTimeOffset.TryParseExact(input, "HHmmss", null, DateTimeStyles.AllowWhiteSpaces, out result)
                ;
        }

        public static DateTimeOffset Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;
            throw new FormatException($"{nameof(DateTimeOffsetParser)}. Failed to parse from string '{input}'.");
        }

        private static bool TryParseDatetimeOffset(string value, string dateSeparator, out DateTimeOffset dt)
        {
            var formatInfo = new DateTimeFormatInfo {DateSeparator = dateSeparator, TimeSeparator = ":"};
            return DateTimeOffset.TryParse(value, formatInfo, DateTimeStyles.AllowWhiteSpaces, out dt);
        }
    }
}