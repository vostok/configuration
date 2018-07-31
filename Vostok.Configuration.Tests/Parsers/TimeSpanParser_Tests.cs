using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    public class TimeSpanParser_Tests
    {
        [TestCase("1d", true, 1, 0, 0, 0, 0)]
        [TestCase("1 day", true, 1, 0, 0, 0, 0)]
        [TestCase("1   days", true, 1, 0, 0, 0, 0)]
        [TestCase("1h", true, 0, 1, 0, 0, 0)]
        [TestCase("1 hour", true, 0, 1, 0, 0, 0)]
        [TestCase("1   hours", true, 0, 1, 0, 0, 0)]
        [TestCase("1m", true, 0, 0, 1, 0, 0)]
        [TestCase("1 min", true, 0, 0, 1, 0, 0)]
        [TestCase("1 minute", true, 0, 0, 1, 0, 0)]
        [TestCase("1   minutes", true, 0, 0, 1, 0, 0)]
        [TestCase("1s", true, 0, 0, 0, 1, 0)]
        [TestCase("1 sec", true, 0, 0, 0, 1, 0)]
        [TestCase("1 second", true, 0, 0, 0, 1, 0)]
        [TestCase("1   seconds", true, 0, 0, 0, 1, 0)]
        [TestCase("1ms", true, 0, 0, 0, 0, 1)]
        [TestCase("1 msec", true, 0, 0, 0, 0, 1)]
        [TestCase("1 millisecond", true, 0, 0, 0, 0, 1)]
        [TestCase("1   milliseconds", true, 0, 0, 0, 0, 1)]
        [TestCase("1 km", false, 0, 0, 0, 0, 0)]
        public void Should_TryParse(string val, bool boolRes, int d, int h, int m, int s, int ms)
        {
            TimeSpanParser.TryParse(val, out var res).Should().Be(boolRes && res == new TimeSpan(d, h, m, s, ms));
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => TimeSpanParser.Parse("1 kg")).Should().Throw<FormatException>();
        }
    }
}