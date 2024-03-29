﻿using System;
using JetBrains.Annotations;
using Vostok.Commons.Helpers;

namespace Vostok.Configuration.Parsers
{
    [PublicAPI]
    public static class TimeSpanParser
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
            input = PrepareInput(input);

            if (TimeSpan.TryParse(input, out result))
                return true;

            bool TryParse(string unit, out double res) => NumericTypeParser<double>.TryParse(PrepareInput(input, unit), out res);

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

            bool TryGet(FromValue method, string unit, out TimeSpan res)
            {
                res = default;
                if (!input.Contains(unit)) return false;
                if (!TryParse(unit, out var val)) return false;
                res = method(val);
                return true;
            }
        }

        private static string PrepareInput(string input, string unit) =>
            input.Replace(unit, string.Empty).Trim('.').Trim();

        private static string PrepareInput(string input) =>
            input.ToLower().Replace(',', '.');

        private delegate TimeSpan FromValue(double value);
    }
}