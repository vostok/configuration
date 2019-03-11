using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    public class PrimitiveBinder_Tests : TreeConstructionSet
    {
        private PrimitiveBinder<int> binder;

        [SetUp]
        public void TestSetup()
        {
            binder = new PrimitiveBinder<int>(new InlineTypeParser<int>(int.TryParse));
        }

        [Test]
        public void Should_bind_value_node()
        {
            binder.Bind(Value("42")).Value.Should().Be(42);
        }

        [Test]
        public void Should_bind_array_node_with_single_child()
        {
            binder.Bind(Array(Value("42"))).Value.Should().Be(42);
        }

        [Test]
        public void Should_bind_object_node_with_single_child()
        {
            binder.Bind(Object(("value", "42"))).Value
                .Should().Be(42);
        }

        [Test]
        public void Should_treat_node_with_single_null_value_child_as_null()
        {
            binder.IsNullValue(Object(Value("xx", null))).Should().BeTrue();
            binder.IsNullValue(Array(Value(null))).Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_parser_returns_false()
        {
            new Func<int>(() => binder.Bind(Value("xx")).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
        
        [Test]
        public void Should_return_default_value_for_missing_nodes()
        {
            binder.Bind(null).Value.Should().Be(0);
        }

        [Test]
        public void Should_return_default_value_for_null_value_nodes()
        {
            binder.Bind(Value(null)).Value.Should().Be(0);
            binder.Bind(Array("xx", Value(null))).Value.Should().Be(0);
        }

        [Test]
        public void Should_treat_null_literal_as_null_for_ref_types()
        {
            new PrimitiveBinder<object>(null).IsNullValue(Value("null")).Should().BeTrue();
            new PrimitiveBinder<object>(null).IsNullValue(Value("NULL")).Should().BeTrue();
        }

        [Test]
        public void Should_not_treat_null_literal_as_null_for_value_types()
        {
            new PrimitiveBinder<int>(null).IsNullValue(Value("null")).Should().BeFalse();
            new PrimitiveBinder<int>(null).IsNullValue(Value("NULL")).Should().BeFalse();
        }

        [Test]
        public void Should_not_treat_null_literal_as_null_for_string()
        {
            new PrimitiveBinder<string>(null).IsNullValue(Value("null")).Should().BeFalse();
            new PrimitiveBinder<string>(null).IsNullValue(Value("NULL")).Should().BeFalse();
        }
    }
}