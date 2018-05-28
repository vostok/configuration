using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniFileSource_Tests
    {
        private const string TestFileName1 = "test_IniFileSource_1.ini";
        private const string TestFileName2 = "test_IniFileSource_2.ini";

        [SetUp]
        public void SetUp()
        {
            Cleanup();
        }

        [TearDown]
        public void Cleanup()
        {
            DeleteFiles();
        }

        private static void CreateTextFile(string text, int n = 1)
        {
            var name = string.Empty;
            switch (n)
            {
                case 1: name = TestFileName1; break;
                case 2: name = TestFileName2; break;
            }

            using (var file = new StreamWriter(name, false))
                file.WriteLine(text);
        }

        private static void DeleteFiles()
        {
            File.Delete(TestFileName1);
            File.Delete(TestFileName2);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var ifs = new IniFileSource("some_file"))
                ifs.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            CreateTextFile("value = 123 \n value2 = 321");

            using (var ifs = new IniFileSource(TestFileName1))
            {
                var result = ifs.Get();
                result["value"].Value.Should().Be("123");
                result["value2"].Value.Should().Be("321");
            }
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            CreateTextFile("wrong file format", 2);
            new Action(() =>
            {
                using (var ifs = new IniFileSource(TestFileName2))
                    ifs.Get();
            }).Should().Throw<Exception>();
        }
    }
}