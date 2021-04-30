using System;
using System.Globalization;

namespace Vostok.Configuration.Validation
{
    internal static class TimespanFormatter
    {
        public static string Format(TimeSpan time)
        {
            if (time.TotalDays >= 1)
                return time.TotalDays.ToString("0.###", CultureInfo.InvariantCulture) + " days";

            if (time.TotalHours >= 1)
                return time.TotalHours.ToString("0.###", CultureInfo.InvariantCulture) + " hours";

            if (time.TotalMinutes >= 1)
                return time.TotalMinutes.ToString("0.###", CultureInfo.InvariantCulture) + " minutes";

            if (time.TotalSeconds >= 1)
                return time.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " seconds";

            if (time.TotalMilliseconds >= 1)
                return time.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture) + " milliseconds";

            return time.ToString();
        }
    }
}