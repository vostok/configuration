using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Binders.Results;

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
            stringBinder.Bind(Arg.Any<ISettingsNode>())
                .Returns(callInfo => 
                    callInfo.Arg<ISettingsNode>()?.Value != "BAD" ? 
                        SettingsBindingResult.Success(callInfo.Arg<ISettingsNode>()?.Value) :
                        SettingsBindingResult.Error<string>(":("));

            binder = new SetBinder<string>(stringBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(null, "a", "b", "c", "a");

            binder.Bind(settings).UnwrapIfNoErrors().Should().BeEquivalentTo("a", "b", "c");
        }

        [Test]
        public void Should_bind_arrays_without_items()
        {
            var settings = Array(new string[] {});

            binder.Bind(settings).UnwrapIfNoErrors().Should().BeEmpty();
        }

        [Test]
        public void Should_report_errors_from_inner_binder()
        {
            var settings = Array(null, "xxx", "BAD");

            new Action(() => binder.Bind(settings).UnwrapIfNoErrors())
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
    }
}