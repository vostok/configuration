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
    public class DictionaryBinder_Tests
    {
        private DictionaryBinder<string, bool> binder;
        private ISettingsBinder<string> stringBinder;

        [SetUp]
        public void TestSetup()
        {
            stringBinder = Substitute.For<ISettingsBinder<string>>();
            stringBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => callInfo.Arg<ISettingsNode>().Value);

            var boolBinder = Substitute.For<ISettingsBinder<bool>>();
            boolBinder.Bind(Arg.Any<ISettingsNode>()).Returns(callInfo => (callInfo.Arg<ISettingsNode>() as ValueNode)?.Value == "true" ? true : throw new BindingException(""));

            binder = new DictionaryBinder<string, bool>(stringBinder, boolBinder);
        }

        [Test]
        public void Should_bind_arrays_with_items()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("key1", "true"),
                new ValueNode("key2", "true"),
            });

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
            var settings = new ArrayNode(new List<ISettingsNode>());

            binder.Bind(settings).Should().BeEmpty();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("key1", "true"),
                new ValueNode("key2", "xxx"),
            });

            new Action(() => binder.Bind(settings)).Should().Throw<BindingException>();
        }
    }
}