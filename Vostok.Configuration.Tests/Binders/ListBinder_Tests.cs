/*using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ListBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_List_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<List<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ICollection_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<ICollection<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IEnumerable_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IEnumerable<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IList_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyList_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyCollection_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings("1"),
                ["2"] = new RawSettings("2"),
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyCollection<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_list_of_lists_of_primitives()
        {
            var settings = new RawSettings(new OrderedDictionary
            {
                ["1"] = new RawSettings(new OrderedDictionary
                {
                    ["1"] = new RawSettings("10"),
                }),
                ["2"] = new RawSettings(new OrderedDictionary
                {
                    ["2"] = new RawSettings("12"),
                }),
            });
            var binder = Container.GetInstance<ISettingsBinder<List<List<int>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<List<int>> { new List<int> {10}, new List<int> {12} });
        }
    }
}*/