using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonSerializer_Tests
    {
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
            JsonSerializer.Serialize(obj, SerializeOption.Short)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Should().HaveCount(1);

            JsonSerializer.Serialize(obj, SerializeOption.Readable)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Should().HaveCountGreaterThan(3);
        }

        private class MyClass
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
            public MyClass InnerClass { get; set; }
        }
    }
}