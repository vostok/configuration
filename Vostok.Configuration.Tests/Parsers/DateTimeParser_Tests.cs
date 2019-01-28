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
            DateTimeParser.TryParse("20050809T181142+0330", out var datetime).Should().BeTrue();
            datetime.Should().Be(new DateTime(2005, 8, 9, 18, 11, 42)); // +3:30
            DateTimeParser.TryParse("123", out _).Should().BeFalse();
        }

        [Test]
        public void Should_interpret_dates_without_timezone_as_local()
        {
            DateTimeParser.TryParse("2018-01-28 15:26:00", out var datetime).Should().BeTrue();

            datetime.Should().Be(new DateTime(2018, 1, 28, 15, 26, 0, DateTimeKind.Local));
        }
    }
}