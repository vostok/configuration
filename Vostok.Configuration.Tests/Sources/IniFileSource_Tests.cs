using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniStringSource_Tests
    {
        [Test]
        public void Should_ignore_comments()
        {
            var value = ";comment 1\r\n# comment 2";

            using (var iss = new IniStringSource(value))
                iss.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_throw_FormatException()
        {
            var value = "???";
            using (var iss = new IniStringSource(value))
                new Action(() => iss.Get()).Should().Throw<FormatException>();

            value = "[]";
            using (var iss = new IniStringSource(value))
                new Action(() => iss.Get()).Should().Throw<FormatException>();

            value = " = 123";
            using (var iss = new IniStringSource(value))
                new Action(() => iss.Get()).Should().Throw<FormatException>();

            value = "A.B = 123 \r A.B = 321";
            using (var iss = new IniStringSource(value))
                new Action(() => iss.Get()).Should().Throw<FormatException>();

            value = "a=0 \r a.b=1 \r a.b.c=2 \r a.b=11";
            using (var iss = new IniStringSource(value))
                new Action(() => iss.Get()).Should().Throw<FormatException>();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            var value = "value = 123 \n value2 = 321";

            using (var iss = new IniStringSource(value))
                iss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings("123") },
                            { "value2", new RawSettings("321") },
                        }));
        }
        
        [Test]
        public void Should_parse_simple_sections()
        {
            var value = "[section1]\rvalue=123 \r [section2]\rvalue1=123\rvalue2=321";

            using (var iss = new IniStringSource(value))
                iss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "section1", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "value", new RawSettings("123") },
                                })
                            },
                            { "section2", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "value1", new RawSettings("123") },
                                    { "value2", new RawSettings("321") },
                                })
                            }
                        }));
        }

        [TestCase("a=0 \r a.b.c=2 \r a.b=1")]
        [TestCase("a=0 \r a.b=1 \r a.b.c=2")]
        [TestCase("a.b.c=2 \r a.b=1 \r a=0")]
        public void Should_deep_parse_keys_with_different_order(string value)
        {
            using (var iss = new IniStringSource(value))
                iss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "a", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "b", new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "c", new RawSettings("2") },
                                        }, "1")
                                    },
                                }, "0")
                            },
                        }));
        }
    }
}