using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonSerializer_Tests
    {
        private const string TestFileName = "test_JsonSerializer.json";

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFileName);
        }

        [Test]
        public void Should_serialize_json()
        {
            var obj = new MyClass
            {
                IntValue = 123,
                StringValue = "qwe",
                InnerClass = new MyClass
                {
                    IntValue = 321,
                    StringValue = "ewq",
                }
            };
            JsonSerializer.Serialize(obj, TestFileName, SerializeOption.Short);
            File.ReadAllText(TestFileName).Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(1);

            JsonSerializer.Serialize(obj, TestFileName, SerializeOption.Readable);
            File.ReadAllText(TestFileName).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Should().HaveCountGreaterThan(3);
        }

        private class MyClass
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
            public MyClass InnerClass { get; set; }
        }
    }
}