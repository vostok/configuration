using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Parsers;

namespace Vostok.Commons.Tests.Parsers
{
    [TestFixture]
    public class IPEndPointParser_Tests
    {
        [Test]
        public void Should_TryParse()
        {
            IPEndPoint res;
            IPEndPointParser.TryParse("127.0.0.1", out res).Should().BeTrue();
            res.Should().BeEquivalentTo(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 0));

            IPEndPointParser.TryParse("192.168.1.10:80", out res).Should().BeTrue();
            res.Should().BeEquivalentTo(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 1, 10 }), 80));

            var ipV6 = "2001:0db8:11a3:09d7:1f34:8a2e:07a0:765d";
            var ipV6Bytes = new byte[] { 32, 1, 13, 184, 17, 163, 9, 215, 31, 52, 138, 46, 7, 160, 118, 93 };

            IPEndPointParser.TryParse(ipV6, out res).Should().BeTrue();
            res.Should().BeEquivalentTo(new IPEndPoint(new IPAddress(ipV6Bytes), 0));

            IPEndPointParser.TryParse($"[{ipV6}]", out res).Should().BeTrue();
            res.Should().BeEquivalentTo(new IPEndPoint(new IPAddress(ipV6Bytes), 0));

            IPEndPointParser.TryParse($"[{ipV6}]:80", out res).Should().BeTrue();
            res.Should().BeEquivalentTo(new IPEndPoint(new IPAddress(ipV6Bytes), 80));
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => IPEndPointParser.Parse("900.0.0.1:5")).Should().Throw<FormatException>();
        }
    }
}