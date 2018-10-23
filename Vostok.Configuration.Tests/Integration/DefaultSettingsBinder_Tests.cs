using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Integration
{
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;

        [SetUp]
        public void TestSetup()
        {
            binder = new DefaultSettingsBinder();
        }

        [TestCase("42", 42)]
        [TestCase("text", "text")]
        [TestCase("Blue", ConsoleColor.Blue)]
        public void Should_bind_single_value_node_to_naked_type<T>(string rawValue, T expectedValue)
        {
            var tree = Value(rawValue);

            binder.Bind<T>(tree).Should().Be(expectedValue);
        }

        [TestCase("42", 42)]
        public void Should_bind_single_value_node_to_naked_type(string rawValue, int? expectedValue)
        {
            var tree = Value(rawValue);

            binder.Bind<int?>(tree).Should().Be(expectedValue);
        }

        // TODO(krait): Setup cases for other tests for naked type handling.

        [Test]
        public void Should_bind_named_single_value_node_to_naked_type()
        {
            var tree = Value("Name", "42");

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_array_node_with_single_child_to_naked_type()
        {
            var tree = Array(Value("42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_named_array_node_with_single_child_to_naked_type()
        {
            var tree = Array("Name", Value("42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_object_node_with_single_child_to_naked_type()
        {
            var tree = Object(("Key", "42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_named_object_node_with_single_child_to_naked_type()
        {
            var tree = Object("Name", ("Key", "42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        // TODO(krait): Separate test cases.

        [Test]
        public void Should_bind_complex_stuff()
        {
            var tree = Array(
                Object(
                    Object("innerObject", Array("anotherArray", "167")),
                    Array("innerArray", "Yellow", "Red", "Black"),
                    Value("innerRegex", @"\d+")
                ),
                Value(null)
            );

            var result = binder.WithParserFor<Regex>(TryParseRegex).Bind<IEnumerable<ComplexConfig>>(tree);

            result.Should().HaveCount(2);
            result.Last().Should().BeNull();

            result.First().StringWithDefault.Should().Be("default");
            result.First().InnerRegex.ToString().Should().Be(@"\d+");
            result.First().InnerArray.Should().Equal(ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Black);
            result.First().InnerObject.AnotherArray.Should().Equal(167);
        }

        private bool TryParseRegex(string s, out Regex regex)
        {
            regex = new Regex(s);
            return true;
        }

        private class ComplexConfig
        {
            public Regex InnerRegex;

            public List<ConsoleColor> InnerArray;

            public string StringWithDefault = "default";

            public InnerConfig InnerObject;

            public class InnerConfig
            {
                public int[] AnotherArray { get; set; }
            }
        }


        #region Tree construction

        private ValueNode Value(string name, string value) => new ValueNode(name, value);

        private ValueNode Value(string value) => new ValueNode(value);

        private ArrayNode Array(string name, params ISettingsNode[] children) => new ArrayNode(name, children);

        private ArrayNode Array(params ISettingsNode[] children) => new ArrayNode(children);

        private ArrayNode Array(string name, params string[] children) => new ArrayNode(name, children.Select(e => new ValueNode(e)).ToArray());

        private ArrayNode Array(params string[] children) => new ArrayNode(children.Select(e => new ValueNode(e)).ToArray());

        private ObjectNode Object(string name, params ISettingsNode[] children) => new ObjectNode(name, children.ToDictionary(e => e.Name, e => e, StringComparer.InvariantCultureIgnoreCase)); // TODO(krait): Shouldn't we force using ignorecase comparer for ObjectNode children?

        private ObjectNode Object(params ISettingsNode[] children) => new ObjectNode(children.ToDictionary(e => e.Name, e => e, StringComparer.InvariantCultureIgnoreCase));

        private ObjectNode Object(string name, params (string key, string value)[] children) => new ObjectNode(name, children.ToDictionary(e => e.key, e => new ValueNode(e.value) as ISettingsNode, StringComparer.InvariantCultureIgnoreCase));

        private ObjectNode Object(params (string key, string value)[] children) => new ObjectNode(children.ToDictionary(e => e.key, e => new ValueNode(e.value) as ISettingsNode, StringComparer.InvariantCultureIgnoreCase));

        #endregion
    }
}