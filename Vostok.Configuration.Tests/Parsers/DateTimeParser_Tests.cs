using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    public class DateTimeParser_Tests
    {
        [Test]
        public void Should_TryParse()
        {
            DateTimeParser.TryParse("20050809T181142+0330", out var res).Should().BeTrue().And.Be(res == new DateTime(2005, 8, 9, 14, 41, 42));
            DateTimeParser.TryParse("123", out _).Should().BeFalse();
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => DateTimeParser.Parse("123")).Should().Throw<FormatException>();
        }
    }
}