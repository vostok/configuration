using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    public class PrimitiveBinder_Tests
    {
        private PrimitiveBinder<int> binder;

        [SetUp]
        public void TestSetup()
        {
            binder = new PrimitiveBinder<int>(
                new Dictionary<Type, ITypeParser>
                {
                    {typeof(int), new InlineTypeParser<int>(int.TryParse)}
                });
        }

        [Test]
        public void Should_bind_value_node()
        {
            binder.Bind(new ValueNode("42")).Should().Be(42);
        }

        [Test]
        public void Should_bind_array_node_with_single_child()
        {
            binder.Bind(new ArrayNode(new[] {new ValueNode("42")})).Should().Be(42);
        }

        [Test]
        public void Should_bind_object_node_with_single_child()
        {
            binder.Bind(new ObjectNode(new Dictionary<string, ISettingsNode> {{"value", new ValueNode("42")}}))
                .Should().Be(42);
        }

        [Test]
        public void Should_throw_if_parser_returns_false()
        {
            new Action(() => binder.Bind(new ValueNode("xx")))
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_parser_throws()
        {
            new Action(() => binder.Bind(new ValueNode(null)))
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_there_is_no_parser_for_type()
        {
            new Action(() => new PrimitiveBinder<int>(new Dictionary<Type, ITypeParser>()).Bind(new ValueNode("1")))
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
    }
}