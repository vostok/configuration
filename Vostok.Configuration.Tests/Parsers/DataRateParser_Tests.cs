using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    internal class DataRateParser_Tests
    {
        [TestCase("10 /s", true, 10)]
        [TestCase("10/sec", true, 10)]
        [TestCase("10 /second", true, 10)]
        [TestCase("10 |second", false, 0)]
        public void Should_TryParse(string input, bool boolRes, int seconds)
        {
            DataRateParser.TryParse(input, out var res).Should()
                .Be(boolRes && res.BytesPerSecond == seconds);
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => DataRateParser.Parse(@"10 \s")).Should().Throw<FormatException>();
        }
    }
}