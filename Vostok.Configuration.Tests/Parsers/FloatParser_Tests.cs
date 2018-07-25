using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Parsers;

namespace Vostok.Commons.Tests.Parsers
{
    [TestFixture]
    public class FloatParser_Tests
    {
        [TestCase("1.23", true, 1.23f)]
        [TestCase("1,23", true, 1.23f)]
        [TestCase(" -1 000,23 ", true, -1000.23f)]
        [TestCase("5,12e2", true, 512f)]
        [TestCase("5'12e-2", true, 0.0512f)]
        [TestCase("abc", false, 0f)]
        public void Should_TryParse(string input, bool boolRes, float val)
        {
            FloatParser.TryParse(input, out var res).Should().Be(boolRes && Math.Abs(res - val) < 0.00000001);
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => FloatParser.Parse(@"cba")).Should().Throw<FormatException>();
        }
    }
}