using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class NullableBinder_Tests
    {
        private NullableBinder<int> binder;
        private ISettingsBinder<int> innerBinder;

        [SetUp]
        public void TestSetup()
        {
            innerBinder = Substitute.For<ISettingsBinder<int>>();
            innerBinder.Bind(Arg.Any<ISettingsNode>()).Returns(42);

            binder = new NullableBinder<int>(innerBinder);
        }

        [Test]
        public void Should_bind_value_using_inner_binder()
        {
            binder.Bind(new ValueNode("42")).Should().Be(42);
        }

        [Test]
        public void Should_return_null_if_value_is_null()
        {
            binder.Bind(new ValueNode(null)).Should().BeNull();
            innerBinder.DidNotReceive().Bind(Arg.Any<ISettingsNode>());
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            innerBinder.Bind(Arg.Any<ISettingsNode>()).Throws<Exception>();

            new Action(() => binder.Bind(new ValueNode(""))).Should().Throw<Exception>().Which.ShouldBePrinted();
        }
    }
}