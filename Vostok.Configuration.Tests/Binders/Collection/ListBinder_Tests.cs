using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;

namespace Vostok.Configuration.Tests.Binders.Collection
{
    public class ListBinder_Tests
    {
        private ListBinder<bool> binder;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? true : throw new BindingException(""));

            binder = new ListBinder<bool>(boolBinder);
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
            var settings = new ArrayNode(new List<ISettingsNode> { new ValueNode("xxx") });

            new Action(() => binder.Bind(settings)).Should().Throw<BindingException>();
        }

        [Test]
        public void Should_return_mutable_collection_as_ICollection()
        {
            var myBinder = binder as ISettingsBinder<ICollection<bool>>;
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("true"),
                new ValueNode("true"),
            });

            var collection = myBinder.Bind(settings);

            collection.Add(true);
            collection.Should().Equal(true, true, true);
        }
    }
}