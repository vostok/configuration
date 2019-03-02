using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    internal class ParseMethodParser_Tests
    {
        [Test]
        public void Should_work_with_parse_method_provided_by_ParseMethodFinder()
        {
            var parser = new ParseMethodParser(ParseMethodFinder.FindParseMethod(typeof(int)));

            parser.TryParse("12", out var value).Should().BeTrue();

            value.Should().Be(12);
        }
    }
}