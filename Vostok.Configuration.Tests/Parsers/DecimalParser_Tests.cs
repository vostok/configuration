using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    public class DecimalParser_Tests
    {
        [Test]
        public void Should_TryParse()
        {
            decimal res;
            DecimalParser.TryParse("1.23", out res).Should().BeTrue().And.Be(res == 1.23m);
            DecimalParser.TryParse("1,23", out res).Should().BeTrue().And.Be(res == 1.23m);
            DecimalParser.TryParse(" -1 000'23 ", out res).Should().BeTrue().And.Be(res == -1000.23m);
            DecimalParser.TryParse("abc", out _).Should().BeFalse();
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => DecimalParser.Parse(@"cba")).Should().Throw<FormatException>();
        }
    }
}