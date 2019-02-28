using System;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Primitives;

namespace Vostok.Configuration.Parsers
{
    internal static class DataSizeParser
    {
        private const string Bytes1 = "b";
        private const string Bytes2 = "bytes";

        private const string Kilobytes1 = "kb";
        private const string Kilobytes2 = "kilobytes";

        private const string Megabytes1 = "mb";
        private const string Megabytes2 = "megabytes";

        private const string Gigabytes1 = "gb";
        private const string Gigabytes2 = "gigabytes";

        private const string Terabytes1 = "tb";
        private const string Terabytes2 = "terabytes";

        private const string Petabytes1 = "pb";
        private const string Petabytes2 = "petabytes";

        public static bool TryParse(string input, out DataSize result)
        {
            input = input.ToLower();

            bool TryParse(string unit, out double res) => NumericTypeParser<double>.TryParse(PrepareInput(input, unit), out res);
            bool TryParseLong(string unit, out long res) => long.TryParse(PrepareInput(input, unit), out res);

            bool TryGet(FromDouble method, string unit, out DataSize res)
            {
                res = default;
                if (!input.Contains(unit)) return false;
                if (!TryParse(unit, out var val)) return false;
                res = method(val);
                return true;
            }

            bool TryGetL(FromLong method, string unit, out DataSize res)
            {
                res = default;
                if (!input.Contains(unit)) return false;
                if (!TryParseLong(unit, out var val)) return false;
                res = method(val);
                return true;
            }

            if (TryGet(DataSize.FromPetabytes, Petabytes2, out result)
                || TryGet(DataSize.FromTerabytes, Terabytes2, out result)
                || TryGet(DataSize.FromGigabytes, Gigabytes2, out result)
                || TryGet(DataSize.FromMegabytes, Megabytes2, out result)
                || TryGet(DataSize.FromKilobytes, Kilobytes2, out result)
                || TryGetL(DataSize.FromBytes, Bytes2, out result)
                || TryGet(DataSize.FromPetabytes, Petabytes1, out result)
                || TryGet(DataSize.FromTerabytes, Terabytes1, out result)
                || TryGet(DataSize.FromGigabytes, Gigabytes1, out result)
                || TryGet(DataSize.FromMegabytes, Megabytes1, out result)
                || TryGet(DataSize.FromKilobytes, Kilobytes1, out result)
                || TryGetL(DataSize.FromBytes, Bytes1, out result))
                return true;

            if (long.TryParse(input, out var bytes))
            {
                result = DataSize.FromBytes(bytes);
                return true;
            }

            return false;
        }

        public static DataSize Parse(string input)
        {
            if (TryParse(input, out var res))
                return res;

            throw new FormatException($"{nameof(DataSizeParser)}: failed to parse {nameof(DataSize)} from string '{input}'.");
        }

        private static string PrepareInput(string input, string unit) =>
            input.Replace(unit, string.Empty).Trim('.').Trim();

        private delegate DataSize FromDouble(double value);

        private delegate DataSize FromLong(long value);
    }
}