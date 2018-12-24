using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Collection;

namespace Vostok.Configuration.Tests.Binders.Collection
{
    public class SetBinder_Tests : TreeConstructionSet
    {
        private SetBinder<string> binder;
        private ISettingsBinder<string> stringBinder;

        [SetUp]
        public void TestSetup()
        {
            stringBinder = Substitute.For<ISettingsBinder<string>>();
            stringBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => callInfo.Arg<ISettingsNode>()?.Value);

            binder = new SetBinder<string>(stringBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(null, "a", "b", "c", "a");

            binder.Bind(settings).Should().BeEquivalentTo("a", "b", "c");
        }

        [Test]
        public void Should_bind_arrays_without_items()
        {
            var settings = Array(new string[] {});

            binder.Bind(settings).Should().BeEmpty();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            stringBinder.Bind(Arg.Any<ISettingsNode>()).Throws<Exception>();
            var settings = Array(null, "xxx");

            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }
    }
}