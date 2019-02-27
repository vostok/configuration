using System;
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
    public class ReadOnlyListBinder_Tests : TreeConstructionSet
    {
        private ReadOnlyListBinder<bool> binder;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISafeSettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>())
                .Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? 
                    SettingsBindingResult.Success(true) : SettingsBindingResult.Error<bool>(":("));

            binder = new ReadOnlyListBinder<bool>(boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(null, "true", "true");

            binder.Bind(settings).Value.Should().Equal(true, true);
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
            var settings = Array(null, "xxx");

            new Func<bool[]>(() => binder.Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_empty_array_for_missing_nodes()
        {
            binder.Bind(null).Value.Should().BeEmpty();
        }

        [Test]
        public void Should_return_empty_array_for_null_value_nodes()
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