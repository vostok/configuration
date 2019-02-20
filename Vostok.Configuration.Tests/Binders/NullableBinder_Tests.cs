using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Tests.Binders
{
    public class NullableBinder_Tests : TreeConstructionSet
    {
        private NullableBinder<int> binder;
        private ISettingsBinder<int> innerBinder;

        [SetUp]
        public void TestSetup()
        {
            innerBinder = Substitute.For<ISettingsBinder<int>>();
            innerBinder.Bind(Arg.Any<ISettingsNode>()).Returns(SettingsBindingResult.Success(42));

            binder = new NullableBinder<int>(innerBinder);
        }

        [Test]
        public void Should_bind_value_using_inner_binder()
        {
            binder.Bind(Value("42")).Should().Be(42);
        }

        [Test]
        public void Should_bind_missing_node_to_default_value()
        {
            binder.Bind(null).Should().BeNull();
            innerBinder.DidNotReceive().Bind(Arg.Any<ISettingsNode>());
        }

        [Test]
        public void Should_bind_null_value_node_to_default_value()
        {
            binder.Bind(Value(null)).Should().BeNull();
            innerBinder.DidNotReceive().Bind(Arg.Any<ISettingsNode>());
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            innerBinder.Bind(Arg.Any<ISettingsNode>()).Throws<Exception>();

            new Action(() => binder.Bind(Value(""))).Should().Throw<Exception>();
        }
    }
}