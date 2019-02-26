using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Tests.Binders.Collection
{
    public class DictionaryBinder_Tests : TreeConstructionSet
    {
        private DictionaryBinder<string, bool> binder;
        private ISafeSettingsBinder<string> stringBinder;

        [SetUp]
        public void TestSetup()
        {
            stringBinder = Substitute.For<ISafeSettingsBinder<string>>();
            stringBinder.Bind(Arg.Any<ISettingsNode>())
                .Returns(callInfo => SettingsBindingResult.Success(callInfo.Arg<ISettingsNode>().Value));

            var boolBinder = Substitute.For<ISafeSettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>())
                .Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? 
                    SettingsBindingResult.Success(true) : SettingsBindingResult.Error<bool>(":("));

            binder = new DictionaryBinder<string, bool>(stringBinder, boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(Value("key1", "true"), Value("key2", "true"));

            binder.Bind(settings).UnwrapIfNoErrors().Should().BeEquivalentTo(
                    new Dictionary<string, bool>
                    {
                        {"key1", true},
                        {"key2", true},
                    });
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
            var settings = Array(Value("key1", "true"), Value("key2", "xxx"));

            new Action(() => binder.Bind(settings).UnwrapIfNoErrors())
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }
    }
}