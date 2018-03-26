using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniFileSource_Tests
    {
        private const string TestFileName = "test_IniFileSource.ini";

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFileName);
        }

        private static void CreateTextFile(string text)
        {
            using (var file = new StreamWriter(TestFileName, false))
                file.WriteLine(text);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var ifs = new IniFileSource(TestFileName))
                ifs.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_ignore_comments()
        {
            CreateTextFile(";comment 1\r\n# comment 2");

            using (var ifs = new IniFileSource(TestFileName))
                ifs.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_throw_FormatException()
        {
            CreateTextFile("???");
            using (var ifs = new IniFileSource(TestFileName))
                new Action(() => ifs.Get()).Should().Throw<FormatException>();

            CreateTextFile("[]");
            using (var ifs = new IniFileSource(TestFileName))
                new Action(() => ifs.Get()).Should().Throw<FormatException>();

            CreateTextFile(" = 123");
            using (var ifs = new IniFileSource(TestFileName))
                new Action(() => ifs.Get()).Should().Throw<FormatException>();

            CreateTextFile("A.B = 123 \r A.B = 321");
            using (var ifs = new IniFileSource(TestFileName))
                new Action(() => ifs.Get()).Should().Throw<FormatException>();

            CreateTextFile("a=0 \r a.b=1 \r a.b.c=2 \r a.b=11");
            using (var ifs = new IniFileSource(TestFileName))
                new Action(() => ifs.Get()).Should().Throw<FormatException>();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            CreateTextFile("value = 123 \n value2 = 321");

            using (var ifs = new IniFileSource(TestFileName))
                ifs.Get().Should().BeEquivalentTo(
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
            CreateTextFile("[section1]\rvalue=123 \r [section2]\rvalue1=123\rvalue2=321");

            using (var ifs = new IniFileSource(TestFileName))
                ifs.Get().Should().BeEquivalentTo(
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
        public void Should_deep_parse_keys_with_different_order(string text)
        {
            CreateTextFile(text);

            using (var ifs = new IniFileSource(TestFileName))
                ifs.Get().Should().BeEquivalentTo(
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