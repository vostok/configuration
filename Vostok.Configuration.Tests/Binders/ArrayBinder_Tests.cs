using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ArrayBinder_Tests
    {
        private ArrayBinder<bool[]> binder;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISettingsBinder<object>>();
            boolBinder.Bind(Arg.Is<ISettingsNode>(n => n is ValueNode && ((ValueNode)n).Value == "true")).Returns(true);
            boolBinder.ReturnsForAll<object>(_ => throw new InvalidCastException());

            var factory = Substitute.For<ISettingsBinderProvider>();
            factory.CreateFor(typeof(bool)).Returns(boolBinder);

            binder = new ArrayBinder<bool[]>(factory);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("true"),
                new ValueNode("true"),
            });

            binder.Bind(settings).Should().Equal(true, true);
        }

        [Test]
        public void Should_bind_arrays_without_items()
        {
            var settings = new ArrayNode(new List<ISettingsNode>());

            binder.Bind(settings).Should().BeEmpty();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = new ArrayNode(new List<ISettingsNode> {new ValueNode("xxx")});

            new Action(() => binder.Bind(settings)).Should().Throw<InvalidCastException>();
        }
    }
}