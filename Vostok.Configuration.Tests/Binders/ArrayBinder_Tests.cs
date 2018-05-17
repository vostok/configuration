/*using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SimpleInjector;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ArrayBinder_Tests
    {
        private Container container;

        [SetUp]
        public void SetUp()
        {
            container = new Container();
            container.Register(typeof(ISettingsBinder<>), typeof(PrimitiveAndSimpleBinder));
            container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            container.Register(typeof(ISettingsBinder<>), typeof(ArrayBinder<>));
        }

        [Test]
        public void Should_bind_to_Array_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("TRUE"),
                new RawSettings("false"),
            });
            var binder = container.GetInstance<ISettingsBinder<bool[]>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[] { true, false });
        }

        [Test]
        public void Should_bind_to_ICollection_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            var binder = container.GetInstance<ISettingsBinder<IList>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        /*[Test]
        public void Should_bind_to_IEnumerable_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            var binder = container.GetInstance<ISettingsBinder<IEnumerable<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IList_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            var binder = container.GetInstance<ISettingsBinder<IList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyList_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            var binder = container.GetInstance<ISettingsBinder<IReadOnlyList<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_IReadOnlyCollection_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            var binder = container.GetInstance<ISettingsBinder<IReadOnlyCollection<int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_list_of_lists_of_primitives()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings(new List<RawSettings>
                {
                    new RawSettings("10"),
                }),
                new RawSettings(new List<RawSettings>
                {
                    new RawSettings("12"),
                }),
            });
            var binder = container.GetInstance<ISettingsBinder<List<List<int>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new List<List<int>> { new List<int> {10}, new List<int> {12} });
        }#1#
    }
}*/