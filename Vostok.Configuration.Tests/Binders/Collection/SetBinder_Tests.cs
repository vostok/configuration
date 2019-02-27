using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SetBinder_Tests : TreeConstructionSet
    {
        private SetBinder<string> binder;
        private ISafeSettingsBinder<string> stringBinder;

        [SetUp]
        public void TestSetup()
        {
            stringBinder = Substitute.For<ISafeSettingsBinder<string>>();
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

            binder.Bind(settings).Value.Should().BeEquivalentTo("a", "b", "c");
        }

        [Test]
        public void Should_bind_arrays_without_items()
        {
            var settings = Array(new string[] {});

            binder.Bind(settings).Value.Should().BeEmpty();
        }

        [Test]
        public void Should_report_errors_from_inner_binder()
        {
            var settings = Array(null, "xxx", "BAD");

            new Func<HashSet<string>>(() => binder.Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_empty_set_for_missing_nodes()
        {
            binder.Bind(null).Value.Should().BeEmpty();
        }

        [Test]
        public void Should_return_empty_set_for_null_value_nodes()
        {
            binder.Bind(Value(null)).Value.Should().BeEmpty();
        }

        [Test]
        public void Should_correctly_handle_null_value_values()
        {
            var valueBinder = Substitute.For<ISafeSettingsBinder<string>>();
            valueBinder.Bind(Arg.Any<ISettingsNode>()).Returns(SettingsBindingResult.Success("default"));
            
            var settings = Array(Value(null));
            
            new ListBinder<string>(valueBinder)
                .Bind(settings).Value.Single().Should().BeNull();
        }
    }
}