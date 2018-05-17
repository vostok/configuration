using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Vostok.Commons;
using Vostok.Commons.Parsers;
using UriParser = Vostok.Commons.Parsers.UriParser;

namespace Vostok.Configuration.Binders
{
    internal class PrimitiveAndSimpleBinder :
        ISettingsBinder<bool>,
        ISettingsBinder<byte>,
        ISettingsBinder<char>,
        ISettingsBinder<decimal>,
        ISettingsBinder<double>,
        ISettingsBinder<float>,
        ISettingsBinder<int>,
        ISettingsBinder<long>,
        ISettingsBinder<sbyte>,
        ISettingsBinder<short>,
        ISettingsBinder<uint>,
        ISettingsBinder<ulong>,
        ISettingsBinder<ushort>,
        ISettingsBinder<string>,
        ISettingsBinder<DateTime>,
        ISettingsBinder<DateTimeOffset>,
        ISettingsBinder<TimeSpan>,
        ISettingsBinder<IPAddress>,
        ISettingsBinder<IPEndPoint>,
        ISettingsBinder<Guid>,
        ISettingsBinder<Uri>,
        ISettingsBinder<DataSize>,
        ISettingsBinder<DataRate>,
        ISettingsBinder<bool?>,
        ISettingsBinder<byte?>,
        ISettingsBinder<char?>,
        ISettingsBinder<decimal?>,
        ISettingsBinder<double?>,
        ISettingsBinder<float?>,
        ISettingsBinder<int?>,
        ISettingsBinder<long?>,
        ISettingsBinder<sbyte?>,
        ISettingsBinder<short?>,
        ISettingsBinder<uint?>,
        ISettingsBinder<ulong?>,
        ISettingsBinder<ushort?>,
        ISettingsBinder<DateTime?>,
        ISettingsBinder<DateTimeOffset?>,
        ISettingsBinder<TimeSpan?>,
        ISettingsBinder<Guid?>,
        ISettingsBinder<DataSize?>,
        ISettingsBinder<DataRate?>
    {
        private readonly Dictionary<Type, ITypeParser> primitiveAndSimpleParsers;

        public PrimitiveAndSimpleBinder()
        {
            primitiveAndSimpleParsers = new Dictionary<Type, ITypeParser>
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
        }

        private T Bind<T>(RawSettings settings)
        {
            bool IsNullable(Type t) => t.IsValueType && t.IsGenericType;

            var type = typeof(T);
            if (!primitiveAndSimpleParsers.ContainsKey(type) && type != typeof(string) && !IsNullable(type))
                throw new ArgumentException("Wrong type");

            if (IsNullable(type))
            {
                if (settings.Value == null)
                    return default;
                type = type.GenericTypeArguments[0];
            }

            string value;
            if (!string.IsNullOrWhiteSpace(settings.Value))
                value = settings.Value;
            else if (settings.Value == null && settings.Children == null && settings.ChildrenByKey != null && settings.ChildrenByKey.Count == 1)
                value = settings.ChildrenByKey.First().Value.Value;
            else
                throw new ArgumentException("Value is null");

            if (type == typeof(string))
                return (T)(object)value;
            if (primitiveAndSimpleParsers[type].TryParse(value, out var res))
                return (T)res;

            throw new InvalidCastException("Wrong type");
        }

        bool ISettingsBinder<bool>.Bind(RawSettings rawSettings) => Bind<bool>(rawSettings);
        byte ISettingsBinder<byte>.Bind(RawSettings rawSettings) => Bind<byte>(rawSettings);
        char ISettingsBinder<char>.Bind(RawSettings rawSettings) => Bind<char>(rawSettings);
        decimal ISettingsBinder<decimal>.Bind(RawSettings rawSettings) => Bind<decimal>(rawSettings);
        double ISettingsBinder<double>.Bind(RawSettings rawSettings) => Bind<double>(rawSettings);
        float ISettingsBinder<float>.Bind(RawSettings rawSettings) => Bind<float>(rawSettings);
        int ISettingsBinder<int>.Bind(RawSettings rawSettings) => Bind<int>(rawSettings);
        long ISettingsBinder<long>.Bind(RawSettings rawSettings) => Bind<long>(rawSettings);
        sbyte ISettingsBinder<sbyte>.Bind(RawSettings rawSettings) => Bind<sbyte>(rawSettings);
        short ISettingsBinder<short>.Bind(RawSettings rawSettings) => Bind<short>(rawSettings);
        uint ISettingsBinder<uint>.Bind(RawSettings rawSettings) => Bind<uint>(rawSettings);
        ulong ISettingsBinder<ulong>.Bind(RawSettings rawSettings) => Bind<ulong>(rawSettings);
        ushort ISettingsBinder<ushort>.Bind(RawSettings rawSettings) => Bind<ushort>(rawSettings);
        string ISettingsBinder<string>.Bind(RawSettings rawSettings) => Bind<string>(rawSettings);
        DateTime ISettingsBinder<DateTime>.Bind(RawSettings rawSettings) => Bind<DateTime>(rawSettings);
        DateTimeOffset ISettingsBinder<DateTimeOffset>.Bind(RawSettings rawSettings) => Bind<DateTimeOffset>(rawSettings);
        TimeSpan ISettingsBinder<TimeSpan>.Bind(RawSettings rawSettings) => Bind<TimeSpan>(rawSettings);
        IPAddress ISettingsBinder<IPAddress>.Bind(RawSettings rawSettings) => Bind<IPAddress>(rawSettings);
        IPEndPoint ISettingsBinder<IPEndPoint>.Bind(RawSettings rawSettings) => Bind<IPEndPoint>(rawSettings);
        Guid ISettingsBinder<Guid>.Bind(RawSettings rawSettings) => Bind<Guid>(rawSettings);
        Uri ISettingsBinder<Uri>.Bind(RawSettings rawSettings) => Bind<Uri>(rawSettings);
        DataSize ISettingsBinder<DataSize>.Bind(RawSettings rawSettings) => Bind<DataSize>(rawSettings);
        DataRate ISettingsBinder<DataRate>.Bind(RawSettings rawSettings) => Bind<DataRate>(rawSettings);
        bool? ISettingsBinder<bool?>.Bind(RawSettings rawSettings) => Bind<bool?>(rawSettings);
        byte? ISettingsBinder<byte?>.Bind(RawSettings rawSettings) => Bind<byte?>(rawSettings);
        char? ISettingsBinder<char?>.Bind(RawSettings rawSettings) => Bind<char?>(rawSettings);
        decimal? ISettingsBinder<decimal?>.Bind(RawSettings rawSettings) => Bind<decimal?>(rawSettings);
        double? ISettingsBinder<double?>.Bind(RawSettings rawSettings) => Bind<double?>(rawSettings);
        float? ISettingsBinder<float?>.Bind(RawSettings rawSettings) => Bind<float?>(rawSettings);
        int? ISettingsBinder<int?>.Bind(RawSettings rawSettings) => Bind<int?>(rawSettings);
        long? ISettingsBinder<long?>.Bind(RawSettings rawSettings) => Bind<long?>(rawSettings);
        sbyte? ISettingsBinder<sbyte?>.Bind(RawSettings rawSettings) => Bind<sbyte?>(rawSettings);
        short? ISettingsBinder<short?>.Bind(RawSettings rawSettings) => Bind<short?>(rawSettings);
        uint? ISettingsBinder<uint?>.Bind(RawSettings rawSettings) => Bind<uint?>(rawSettings);
        ulong? ISettingsBinder<ulong?>.Bind(RawSettings rawSettings) => Bind<ulong?>(rawSettings);
        ushort? ISettingsBinder<ushort?>.Bind(RawSettings rawSettings) => Bind<ushort?>(rawSettings);
        DateTime? ISettingsBinder<DateTime?>.Bind(RawSettings rawSettings) => Bind<DateTime?>(rawSettings);
        DateTimeOffset? ISettingsBinder<DateTimeOffset?>.Bind(RawSettings rawSettings) => Bind<DateTimeOffset?>(rawSettings);
        TimeSpan? ISettingsBinder<TimeSpan?>.Bind(RawSettings rawSettings) => Bind<TimeSpan?>(rawSettings);
        Guid? ISettingsBinder<Guid?>.Bind(RawSettings rawSettings) => Bind<Guid?>(rawSettings);
        DataSize? ISettingsBinder<DataSize?>.Bind(RawSettings rawSettings) => Bind<DataSize?>(rawSettings);
        DataRate? ISettingsBinder<DataRate?>.Bind(RawSettings rawSettings) => Bind<DataRate?>(rawSettings);

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