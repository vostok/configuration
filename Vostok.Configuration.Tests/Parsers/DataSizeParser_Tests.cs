using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;
using Vostok.Configuration.Primitives;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    internal class DataSizeParser_Tests
    {
        [Test]
        public void Should_TryParse()
        {
            DataSizeParser.TryParse("1b", out var res).Should().BeTrue().And.Be(res == DataSize.FromBytes(1));
            DataSizeParser.TryParse("10 bytes", out res).Should().BeTrue().And.Be(res == DataSize.FromBytes(10));

            DataSizeParser.TryParse("1KB", out res).Should().BeTrue().And.Be(res == DataSize.FromKilobytes(1));
            DataSizeParser.TryParse("10 KiLoByTeS", out res).Should().BeTrue().And.Be(res == DataSize.FromKilobytes(10));

            DataSizeParser.TryParse("1Mb", out res).Should().BeTrue().And.Be(res == DataSize.FromMegabytes(1));
            DataSizeParser.TryParse("1030   megabytes", out res).Should().BeTrue().And.Be(res == DataSize.FromMegabytes(1030));

            DataSizeParser.TryParse("1gb", out res).Should().BeTrue().And.Be(res == DataSize.FromGigabytes(1));
            DataSizeParser.TryParse("10  gigabytes", out res).Should().BeTrue().And.Be(res == DataSize.FromGigabytes(10));

            DataSizeParser.TryParse("1tb", out res).Should().BeTrue().And.Be(res == DataSize.FromTerabytes(1));
            DataSizeParser.TryParse("10 terabytes", out res).Should().BeTrue().And.Be(res == DataSize.FromTerabytes(10));

            DataSizeParser.TryParse("1pb", out res).Should().BeTrue().And.Be(res == DataSize.FromPetabytes(1));
            DataSizeParser.TryParse("10 petabytes", out res).Should().BeTrue().And.Be(res == DataSize.FromPetabytes(10));

            DataSizeParser.TryParse("10 bites", out _).Should().BeFalse();
        }

        [Test]
        public void Should_throw_FormatException_on_Parse_wrong_format()
        {
            new Action(() => DataSizeParser.Parse(@"10 bites")).Should().Throw<FormatException>();
        }
    }
}