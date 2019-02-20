using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;

namespace Vostok.Configuration.Tests.Binders.Collection
{
    public class DictionaryBinder_Tests : TreeConstructionSet
    {
        private DictionaryBinder<string, bool> binder;
        private ISettingsBinder<string> stringBinder;

        [SetUp]
        public void TestSetup()
        {
            stringBinder = Substitute.For<ISettingsBinder<string>>();
            stringBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => callInfo.Arg<ISettingsNode>().Value);

            var boolBinder = Substitute.For<ISettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? true : throw new SettingsBindingException(""));

            binder = new DictionaryBinder<string, bool>(stringBinder, boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = Array(Value("key1", "true"), Value("key2", "true"));

            binder.Bind(settings).Should().BeEquivalentTo(
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

            binder.Bind(settings).Should().BeEmpty();
        }
        
        [Test]
        public void Should_bind_missing_node_to_default_value()
        {
            binder.Bind(null).Should().BeNull();
        }

        [Test]
        public void Should_bind_null_value_node_to_default_value()
        {
            binder.Bind(Value(null)).Should().BeNull();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = Array(Value("key1", "true"), Value("key2", "xxx"));

            new Action(() => binder.Bind(settings)).Should().Throw<SettingsBindingException>();
        }
    }
}