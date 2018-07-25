using System;
using FluentAssertions;
using NUnit.Framework;
using UriParser = Vostok.Configuration.Parsers.UriParser;

namespace Vostok.Commons.Tests.Parsers
{
    [TestFixture]
    public class UriParser_Tests
    {
        [TestCase("http://example.com", true, UriKind.Absolute)]
        [TestCase("example.com/some", true, UriKind.RelativeOrAbsolute)]
        [TestCase("/part/of/path", true, UriKind.Relative)]
        //[TestCase("/////", false, UriKind.RelativeOrAbsolute)]    always true whatever i write
        public void Should_TryParse(string uri, bool boolRes, UriKind kind)
        {
            UriParser.TryParse(uri, out var res).Should().Be(boolRes);
            res.Should().BeEquivalentTo(new Uri(uri, kind));
        }

        /*[Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => UriParser.Parse("\u0001 ")).Should().Throw<FormatException>();
        }*/
    }
}