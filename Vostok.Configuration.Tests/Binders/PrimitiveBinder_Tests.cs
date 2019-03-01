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
    }
}