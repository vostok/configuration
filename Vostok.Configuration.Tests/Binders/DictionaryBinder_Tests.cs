using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class DictionaryBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Dictionary_of_primitives()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "1", new ValueNode("10", "1") },
                { "2", new ValueNode("20", "2") },
            });
            var binder = Container.GetInstance<ISettingsBinder<Dictionary<int,int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, int> { { 1, 10 }, { 2, 20 } });
        }

        [Test]
        public void Should_bind_to_IDictionary_of_primitives()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "1", new ValueNode("1.23", "1") },
                { "2", new ValueNode("2.34", "2") },
            });
            var binder = Container.GetInstance<ISettingsBinder<IDictionary<int, double>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, double> { { 1, 1.23 }, { 2, 2.34 } });
        }

        [Test]
        public void Should_bind_to_IReadOnlyDictionary_of_primitives()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "true", new ValueNode("FALSE", "true") },
                { "false", new ValueNode("TRUE", "false") },
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyDictionary<bool, bool>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<bool, bool> { { true, false }, { false, true } });
        }

        [Test]
        public void Should_bind_to_dictionary_of_dictionaries_of_primitives()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "1", new ObjectNode("1", new SortedDictionary<string, ISettingsNode>
                {
                    { "100", new ValueNode("true", "100") },
                }) },
                { "2", new ObjectNode("2", new SortedDictionary<string, ISettingsNode>
                {
                    { "200", new ValueNode("false", "200") },
                }) },
            });
            var binder = Container.GetInstance<ISettingsBinder<Dictionary<int, Dictionary<long, bool>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, Dictionary<long, bool>>
            {
                { 1, new Dictionary<long, bool>
                {
                    { 100, true },
                } },
                { 2, new Dictionary<long, bool>
                {
                    { 200, false },
                } }
            });
        }

        [Test]
        public void Should_throw_exception_if_tree_is_null()
        {
            var binder = Container.GetInstance<ISettingsBinder<Dictionary<int, int>>>();
            new Action(() => binder.Bind(null)).Should().Throw<ArgumentNullException>();
        }
    }
}