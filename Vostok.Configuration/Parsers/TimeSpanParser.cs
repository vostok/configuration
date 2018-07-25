using System;
using Vostok.Commons.Parsers;

namespace Vostok.Configuration.Parsers
{
    internal static class TimeSpanParser
    {
        private const string MilliSeconds1 = "ms";
        private const string MilliSeconds2 = "msec";
        private const string MilliSeconds3 = "millisecond";
        private const string MilliSeconds4 = "milliseconds";

        private const string Seconds1 = "s";
        private const string Seconds2 = "sec";
        private const string Seconds3 = "second";
        private const string Seconds4 = "seconds";

        private const string Minutes1 = "m";
        private const string Minutes2 = "min";
        private const string Minutes3 = "minute";
        private const string Minutes4 = "minutes";

        private const string Hours1 = "h";
        private const string Hours2 = "hour";
        private const string Hours3 = "hours";

        private const string Days1 = "d";
        private const string Days2 = "day";
        private const string Days3 = "days";

        public static bool TryParse(string input, out TimeSpan result)
        {
            input = StringMethods.PrepareForTimeSpan(input);

            if (TimeSpan.TryParse(input, out result))
                return true;

            bool TryParse(string unit, out double res) => DoubleParser.TryParse(PrepareInput(input, unit), out res);

            bool TryGet(FromValue method, string unit, out TimeSpan res)
            {
                if (!input.Contains(unit)) return false;
                if (!TryParse(unit, out var val)) return false;
                res = method(val);
                return true;
            }

            return
                TryGet(TimeSpan.FromMilliseconds, MilliSeconds4, out result) ||
                TryGet(TimeSpan.FromMilliseconds, MilliSeconds3, out result) ||
                TryGet(TimeSpan.FromSeconds, Seconds4, out result) ||
                TryGet(TimeSpan.FromSeconds, Seconds3, out result) ||
                TryGet(TimeSpan.FromMinutes, Minutes4, out result) ||
                TryGet(TimeSpan.FromMinutes, Minutes3, out result) ||
                TryGet(TimeSpan.FromHours, Hours3, out result) ||
                TryGet(TimeSpan.FromHours, Hours2, out result) ||
                TryGet(TimeSpan.FromDays, Days3, out result) ||
                TryGet(TimeSpan.FromDays, Days2, out result) ||
                TryGet(TimeSpan.FromMilliseconds, MilliSeconds2, out result) ||
                TryGet(TimeSpan.FromSeconds, Seconds2, out result) ||
                TryGet(TimeSpan.FromMinutes, Minutes2, out result) ||
                TryGet(TimeSpan.FromMilliseconds, MilliSeconds1, out result) ||
                TryGet(TimeSpan.FromSeconds, Seconds1, out result) ||
                TryGet(TimeSpan.FromMinutes, Minutes1, out result) ||
                TryGet(TimeSpan.FromHours, Hours1, out result) ||
                TryGet(TimeSpan.FromDays, Days1, out result);
        }

        public static TimeSpan Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;
            throw new FormatException($"{nameof(TimeSpanParser)}. Failed to parse from string '{input}'.");
        }

        private delegate TimeSpan FromValue(double value);

        private static string PrepareInput(string input, string unit) =>
            input.Replace(unit, string.Empty).Trim('.').Trim();
    }
}