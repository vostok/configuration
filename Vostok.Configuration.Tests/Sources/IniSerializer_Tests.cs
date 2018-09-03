using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniSerializer_Tests
    {
        [TestCase(123)]
        [TestCase("str")]
        [TestCase(ConsoleColor.Green)]
        public void Should_serialize_simple(object value)
        {
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo($"value = {value}");
        }

        [Test]
        public void Should_serialize_nullable()
        {
            long? value = 321;
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo($"value = {value}");

            value = null;
            res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo("value = ");
        }

        [Test]
        public void Should_serialize_dictionary_of_simples()
        {
            var value = new Dictionary<int, string>
            {
                [100] = "str",
                [200] = "ing",
            };
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo($"100 = {value[100]}", $"200 = {value[200]}");
        }

        [Test]
        public void Should_serialize_class_of_simples()
        {
            var value = new MyClass
            {
                Int = 123,
                String = "string",
            };
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo($"Int = {value.Int}", $"String = {value.String}");
        }

        [Test]
        public void Should_serialize_struct_of_simples()
        {
            var value = new MyStruct
            {
                Int = 123,
                String = "string",
            };
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo($"Int = {value.Int}", $"String = {value.String}");
        }

        [Test]
        public void Should_serialize_complex_object()
        {
            var value = new ComplexClass
            {
                Class = new MyClass
                {
                    Int = 1,
                    String = "x",
                },
                Dict = new Dictionary<string, MyClass>
                {
                    ["a"] = new MyClass
                    {
                        Int = 2,
                        String = "y",
                    },
                    ["b"] = null,
                },
                Int = 123,
            };
            var res = IniSerializer.Serialize(value);
            Split(res).Should().BeEquivalentTo(
                $"Class.Int = {value.Class.Int}",
                $"Class.String = {value.Class.String}",
                $"Dict.a.Int = {value.Dict["a"].Int}",
                $"Dict.a.String = {value.Dict["a"].String}",
                $"Dict.b = {value.Dict["b"]}",
                $"Int = {value.Int}");
        }

        private static IEnumerable<string> Split(string value) =>
            value.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

        private struct MyStruct
        {
            public int Int { get; set; }
            private int IntPrivate => 321;
            public string String { get; set; }
        }

        private class MyClass
        {
            public int Int { get; set; }
            private int IntPrivate { get; set; } = 321;
            public string String { get; set; }
        }

        private class ComplexClass
        {
            public MyClass Class { get; set; }
            public Dictionary<string, MyClass> Dict { get; set; }
            public int? Int { get; set; }
        }
    }
}