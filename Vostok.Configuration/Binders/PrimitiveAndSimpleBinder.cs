using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Vostok.Commons;
using Vostok.Commons.Parsers;
using UriParser = Vostok.Commons.Parsers.UriParser;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveAndSimpleBinder<T> : ISettingsBinder<T>
    {
        private static readonly IDictionary<Type, ITypeParser> PrimitiveAndSimpleParsers = new Dictionary<Type, ITypeParser>
        {
            {typeof(bool), new InlineTypeParser<bool>(bool.TryParse)},
            {typeof(byte), new InlineTypeParser<byte>(byte.TryParse)},
            {typeof(char), new InlineTypeParser<char>(char.TryParse)},
            {typeof(decimal), new InlineTypeParser<decimal>(DecimalParser.TryParse)},
            {typeof(double), new InlineTypeParser<double>(DoubleParser.TryParse)},
            {typeof(float), new InlineTypeParser<float>(FloatParser.TryParse)},
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
            {typeof(DataSize), new InlineTypeParser<DataSize>(DataSizeParser.TryParse)},
            {typeof(DataRate), new InlineTypeParser<DataRate>(DataRateParser.TryParse)},
        };

        public static bool IsAvailableType(Type type) =>
            type.IsPrimitive() ||
            type == typeof(string) ||
            PrimitiveAndSimpleParsers.ContainsKey(type);

        public T Bind(RawSettings settings)
        {
            var type = typeof(T);
            if (!PrimitiveAndSimpleParsers.ContainsKey(type) && type != typeof(string))
                throw new ArgumentException("Wrong type");

            string value;
            if (!string.IsNullOrWhiteSpace(settings.Value))
                value = settings.Value;
            else if (settings.Value == null && settings.Children == null && settings.ChildrenByKey != null && settings.ChildrenByKey.Count == 1)
                value = settings.ChildrenByKey.First().Value.Value;
            else
                throw new ArgumentException("Value is null");

            if (type == typeof(string))
                return (T)(object)value;
            if (PrimitiveAndSimpleParsers[type].TryParse(value, out var res))
                return (T)res;

            throw new InvalidCastException("Wrong type");
        }

        private class InlineTypeParser<TI> : ITypeParser
        {
            private readonly TryParse<TI> parseMethod;

            public InlineTypeParser(TryParse<TI> parseMethod)
            {
                this.parseMethod = parseMethod;
            }

            public bool TryParse(string s, out object value)
            {
                var result = parseMethod(s, out var v);
                value = v;
                return result;
            }
        }
    }
}