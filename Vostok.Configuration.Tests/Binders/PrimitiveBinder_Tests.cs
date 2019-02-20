using System;
using System.Collections.Generic;
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
            binder.Bind(Value("42")).UnwrapIfNoErrors().Should().Be(42);
        }

        [Test]
        public void Should_bind_array_node_with_single_child()
        {
            binder.Bind(Array(Value("42"))).UnwrapIfNoErrors().Should().Be(42);
        }

        [Test]
        public void Should_bind_object_node_with_single_child()
        {
            binder.Bind(Object(("value", "42"))).UnwrapIfNoErrors()
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
            new Action(() => binder.Bind(Value("xx")).UnwrapIfNoErrors())
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
    }
}