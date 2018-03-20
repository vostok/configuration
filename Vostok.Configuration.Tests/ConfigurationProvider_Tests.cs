using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigurationProvider_Tests
    {
        private const string TestFileName = "test.json";

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

        class MyClass
        {
            public int Value { get; set; }
        }
        [Test]
        public void Get_WithSourceFor_should_work_correctly()
        {
            CreateTextFile("{ \"Value\": 123 }");
            new ConfigurationProvider()
                .WithSourceFor<MyClass>(new JsonFileSource(TestFileName))
                .Get<MyClass>()
                .Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }

        [Test]
        public void Get_from_source_should_work_correctly()
        {
            CreateTextFile("{ \"Value\": 123 }");
            new ConfigurationProvider()
                .Get<MyClass>(new JsonFileSource(TestFileName))
                .Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }
    }
}