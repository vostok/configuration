using System;
using FluentAssertions;
using NUnit.Framework;
using UriParser = Vostok.Configuration.Parsers.UriParser;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    public class UriParser_Tests
    {
        [TestCase("http://example.com", UriKind.Absolute)]
        [TestCase("example.com/some", UriKind.RelativeOrAbsolute)]
        [TestCase("/part/of/path", UriKind.Relative)]
        public void TryParse_should_return_true_for_valid_input(string uri, UriKind kind)
        {
            UriParser.TryParse(uri, out var res).Should().BeTrue();
            res.Should().BeEquivalentTo(new Uri(uri, kind));
        }

        [TestCase(null)]
        [TestCase("http://")]
        public void TryParse_should_return_false_for_invalid_input(string input)
        {
            UriParser.TryParse(input, out var _).Should().BeFalse();
        }
    }
}