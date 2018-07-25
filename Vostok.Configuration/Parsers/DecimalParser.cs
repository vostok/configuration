using System;
using System.Globalization;
using Vostok.Commons.Parsers;

namespace Vostok.Configuration.Parsers
{
    internal static class DecimalParser
    {
        public static bool TryParse(string input, out decimal res)
        {
            input = StringMethods.PrepareForFloatNumbers(input);
            return decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out res);
        }

        public static decimal Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;
            throw new FormatException($"{nameof(DecimalParser)}. Error in parsing string {input} to decimal.");
        }
    }
}