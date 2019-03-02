using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Parsers
{
    [TestFixture]
    internal class ParseMethodFinder_Tests
    {
        [Test]
        public void Should_detect_parse_method()
        {
            ParseMethodFinder.FindParseMethod(typeof(int)).Should().NotBeNull();
            ParseMethodFinder.FindParseMethod(typeof(Guid)).Should().NotBeNull();
            ParseMethodFinder.FindParseMethod(typeof(IPAddress)).Should().NotBeNull();
        }

        [Test]
        public void Should_detect_tryparse_method()
        {
            ParseMethodFinder.FindTryParseMethod(typeof(int)).Should().NotBeNull();
            ParseMethodFinder.FindTryParseMethod(typeof(Guid)).Should().NotBeNull();
            ParseMethodFinder.FindTryParseMethod(typeof(IPAddress)).Should().NotBeNull();
        }

        [Test]
        public void Should_not_detect_methods_where_there_are_none()
        {
            ParseMethodFinder.FindParseMethod(GetType()).Should().BeNull();

            ParseMethodFinder.FindTryParseMethod(GetType()).Should().BeNull();
        }
    }
}