﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;
// ReSharper disable AccessToModifiedClosure

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniStringSource_Tests
    {
        [Test]
        public void Should_return_null_if_null_or_whitespace_string()
        {
            using (var iss = new IniStringSource(null))
                iss.Get().Should().BeNull();
            using (var iss = new IniStringSource(" "))
                iss.Get().Should().BeNull();
        }

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
            new Action(() =>
                {
                    using (new IniStringSource(value)) { }
                }).Should().Throw<FormatException>();

            value = "[]";
            new Action(() =>
            {
                using (new IniStringSource(value)) { }
            }).Should().Throw<FormatException>();

            value = " = 123";
            new Action(() =>
            {
                using (new IniStringSource(value)) { }
            }).Should().Throw<FormatException>();

            value = "A.B = 123 \r A.B = 321";
            new Action(() =>
            {
                using (new IniStringSource(value)) { }
            }).Should().Throw<FormatException>();

            value = "a=0 \r a.b=1 \r a.b.c=2 \r a.b=11";
            new Action(() =>
            {
                using (new IniStringSource(value)) { }
            }).Should().Throw<FormatException>();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            const string value = "value = 123 \n value2 = 321";

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
            const string value = "[section1]\rvalue=123 \r [section2]\rvalue1=123\rvalue2=321";

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

        [Test]
        public void Should_subscribe_and_get_parsed_tree()
        {
            new Action(() => ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives()
        {
            const string value = "value = 123 \n value2 = 321";
            var val = 0;

            using (var iss = new IniStringSource(value))
            {
                var sub = iss.Observe().Subscribe(
                    settings =>
                    {
                        val++;
                        settings.Should().BeEquivalentTo(
                            new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "value", new RawSettings("123") },
                                    { "value2", new RawSettings("321") },
                                }));
                    });
                sub.Dispose();
            }

            return val;
        }
    }
}