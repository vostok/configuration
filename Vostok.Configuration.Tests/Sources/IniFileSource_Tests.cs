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
    }
}