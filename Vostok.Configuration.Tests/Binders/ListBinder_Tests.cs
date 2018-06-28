using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class ListBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_List_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<List<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ICollection_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<ICollection<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IEnumerable_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IEnumerable<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IList_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyList_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyCollection_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyCollection<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_list_of_lists_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("10"),
                }),
                new ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("12"),
                }),
            });
            var binder = Container.GetInstance<ISettingsBinder<List<List<int>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<List<int>> { new List<int> {10}, new List<int> {12} });
        }

        [Test]
        public void Should_throw_exception_if_tree_is_null()
        {
            var binder = Container.GetInstance<ISettingsBinder<List<int>>>();
            new Action(() => binder.Bind(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_exception_if_tree_is_empty()
        {
            var binder = Container.GetInstance<ISettingsBinder<List<int>>>();
            new Action(() => binder.Bind(new ArrayNode(name:null))).Should().Throw<ArgumentNullException>();
        }
    }
}