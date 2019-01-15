using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;

namespace Vostok.Configuration.Tests.Binders.Collection
{
    public class ReadOnlyListBinder_Tests : TreeConstructionSet
    {
        private ReadOnlyListBinder<bool> binder;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? true : throw new SettingsBindingException(""));

            binder = new ReadOnlyListBinder<bool>(boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(null, "true", "true");

            binder.Bind(settings).Should().Equal(true, true);
        }

        [Test]
        public void Should_bind_arrays_without_items()
        {
            var settings = Array(new string[] {});

            binder.Bind(settings).Should().BeEmpty();
        }

        [Test]
        public void Should_bind_null_node_to_null()
        {
            binder.Bind(null).Should().BeNull();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = Array(null, "xxx");

            new Action(() => binder.Bind(settings)).Should().Throw<SettingsBindingException>();
        }
    }
}