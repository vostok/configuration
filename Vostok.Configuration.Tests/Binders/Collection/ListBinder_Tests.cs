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
    public class ListBinder_Tests : TreeConstructionSet
    {
        private ListBinder<bool> binder;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISafeSettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>())
                .Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? 
                    SettingsBindingResult.Success(true) : SettingsBindingResult.Error<bool>(":("));

            binder = new ListBinder<bool>(boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(null, "true", "true");

            binder.Bind(settings).UnwrapIfNoErrors().Should().Equal(true, true);
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
            var settings = Array(null, "xxx");

            new Action(() => binder.Bind(settings).UnwrapIfNoErrors())
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_mutable_collection_as_ICollection()
        {
            var myBinder = binder as ISafeSettingsBinder<ICollection<bool>>;
            var settings = Array(null, "true", "true");

            var collection = myBinder.Bind(settings).UnwrapIfNoErrors();

            collection.Add(true);
            collection.Should().Equal(true, true, true);
        }
    }
}