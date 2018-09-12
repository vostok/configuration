using System;
using System.Collections.Generic;
using System.Net;
using Vostok.Commons.Helpers;
using Vostok.Commons.Primitives;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Parsers;
using UriParser = Vostok.Configuration.Parsers.UriParser;

namespace Vostok.Configuration.Extensions
{
    internal static class PrimitiveAndSimpleParsersExtension
    {
        private static readonly IDictionary<Type, ITypeParser> DefaultParsers = new Dictionary<Type, ITypeParser>
        {
            {typeof(bool), new InlineTypeParser<bool>(bool.TryParse)},
            {typeof(byte), new InlineTypeParser<byte>(byte.TryParse)},
            {typeof(char), new InlineTypeParser<char>(char.TryParse)},
            {typeof(decimal), new InlineTypeParser<decimal>(FloatingPointParser<decimal>.TryParse)},
            {typeof(double), new InlineTypeParser<double>(FloatingPointParser<double>.TryParse)},
            {typeof(float), new InlineTypeParser<float>(FloatingPointParser<float>.TryParse)},
            {typeof(int), new InlineTypeParser<int>(int.TryParse)},
            {typeof(long), new InlineTypeParser<long>(long.TryParse)},
            {typeof(sbyte), new InlineTypeParser<sbyte>(sbyte.TryParse)},
            {typeof(short), new InlineTypeParser<short>(short.TryParse)},
            {typeof(uint), new InlineTypeParser<uint>(uint.TryParse)},
            {typeof(ulong), new InlineTypeParser<ulong>(ulong.TryParse)},
            {typeof(ushort), new InlineTypeParser<ushort>(ushort.TryParse)},
            {typeof(DateTime), new InlineTypeParser<DateTime>(DateTimeParser.TryParse)},
            {typeof(DateTimeOffset), new InlineTypeParser<DateTimeOffset>(DateTimeOffsetParser.TryParse)},
            {typeof(TimeSpan), new InlineTypeParser<TimeSpan>(TimeSpanParser.TryParse)},
            {typeof(IPAddress), new InlineTypeParser<IPAddress>(IPAddress.TryParse)},
            {typeof(IPEndPoint), new InlineTypeParser<IPEndPoint>(IPEndPointParser.TryParse)},
            {typeof(Guid), new InlineTypeParser<Guid>(Guid.TryParse)},
            {typeof(Uri), new InlineTypeParser<Uri>(UriParser.TryParse)},
            {typeof(DataSize), new InlineTypeParser<DataSize>(DataSize.TryParse)},
            {typeof(DataRate), new InlineTypeParser<DataRate>(DataRate.TryParse)},
        };

        public static IDictionary<Type, ITypeParser> AddDefaultParsers(this IDictionary<Type, ITypeParser> parsers)
        {
            foreach (var pair in DefaultParsers)
                parsers.Add(pair.Key, pair.Value);
            return parsers;
        }

        public static IDictionary<Type, ITypeParser> AddCustomParser<TParser>(this IDictionary<Type, ITypeParser> parsers, ITypeParser parser)
        {
            parsers.Add(typeof(TParser), parser);
            return parsers;
        }

        public static IDictionary<Type, ITypeParser> AddCustomParser<TParser>(this IDictionary<Type, ITypeParser> parsers, TryParse<TParser> parseMethod)
        {
            parsers.Add(typeof(TParser), new InlineTypeParser<TParser>(parseMethod));
            return parsers;
        }

        private class InlineTypeParser<TI> : ITypeParser
        {
            private readonly TryParse<TI> parseMethod;

            public InlineTypeParser(TryParse<TI> parseMethod) =>
                this.parseMethod = parseMethod;

            public bool TryParse(string s, out object value)
            {
                var result = parseMethod(s, out var v);
                value = v;
                return result;
            }
        }
    }
}