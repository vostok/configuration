using System;
using System.Net;
using Vostok.Commons.Helpers;
using Vostok.Commons.Primitives;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Parsers;
using UriParser = Vostok.Configuration.Parsers.UriParser;

namespace Vostok.Configuration.Extensions
{
    internal static class SettingsBinderProviderExtensions
    {
        public static ISettingsBinderProvider WithDefaultParsers(this ISettingsBinderProvider binderProvider)
        {
            return binderProvider
                .WithParserFor<string>(StringParser.TryParse)
                .WithParserFor<bool>(bool.TryParse)
                .WithParserFor<char>(char.TryParse)
                .WithParserFor<decimal>(NumericTypeParser<decimal>.TryParse)
                .WithParserFor<double>(NumericTypeParser<double>.TryParse)
                .WithParserFor<float>(NumericTypeParser<float>.TryParse)
                .WithParserFor<sbyte>(sbyte.TryParse)
                .WithParserFor<short>(short.TryParse)
                .WithParserFor<int>(int.TryParse)
                .WithParserFor<long>(long.TryParse)
                .WithParserFor<byte>(byte.TryParse)
                .WithParserFor<ushort>(ushort.TryParse)
                .WithParserFor<uint>(uint.TryParse)
                .WithParserFor<ulong>(ulong.TryParse)
                .WithParserFor<DateTime>(DateTimeParser.TryParse)
                .WithParserFor<DateTimeOffset>(DateTimeOffset.TryParse)
                .WithParserFor<TimeSpan>(TimeSpanParser.TryParse)
                .WithParserFor<IPAddress>(IPAddress.TryParse)
                .WithParserFor<IPEndPoint>(IPEndPointParser.TryParse)
                .WithParserFor<Guid>(Guid.TryParse)
                .WithParserFor<Uri>(UriParser.TryParse)
                .WithParserFor<DataSize>(DataSize.TryParse)
                .WithParserFor<DataRate>(DataRate.TryParse);
        }

        public static ISettingsBinderProvider WithParserFor<T>(this ISettingsBinderProvider binderProvider, TryParse<T> parseMethod)
        {
            binderProvider.SetupParserFor<T>(new InlineTypeParser<T>(parseMethod));

            return binderProvider;
        }
    }
}