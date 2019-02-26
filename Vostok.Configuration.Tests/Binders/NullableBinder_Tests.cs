using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Tests.Binders
{
    public class NullableBinder_Tests : TreeConstructionSet
    {
        private NullableBinder<int> binder;
        private ISafeSettingsBinder<int> innerBinder;

        [SetUp]
        public void TestSetup()
        {
            innerBinder = Substitute.For<ISafeSettingsBinder<int>>();
            innerBinder.Bind(Arg.Any<ISettingsNode>()).Returns(
                callInfo => callInfo.Arg<ISettingsNode>()?.Value == "42" ?
                    SettingsBindingResult.Success(42) :
                    SettingsBindingResult.Error<int>(":("));

            binder = new NullableBinder<int>(innerBinder);
        }

        [Test]
        public void Should_bind_value_using_inner_binder()
        {
            binder.Bind(Value("42")).Value.Should().Be(42);
        }

        [Test]
        public void Should_treat_null_literal_as_null_value()
        {
            binder.IsNullValue(Value("null")).Should().BeTrue();
            binder.IsNullValue(Value("NULL")).Should().BeTrue();
        }

        [Test]
        public void Should_report_errors_from_inner_binder()
        {
            new Func<int?>(() => binder.Bind(Value("")).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
    }
}