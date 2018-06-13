using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class SetBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_HashSet_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("10"),
                new ValueNode("20"),
            });
            var binder = Container.GetInstance<ISettingsBinder<HashSet<short>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new HashSet<short> { 10, 20 });
        }

        [Test]
        public void Should_bind_to_ISet_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("10"),
                new ValueNode("20"),
            });
            var binder = Container.GetInstance<ISettingsBinder<ISet<ushort>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new HashSet<ushort> { 10, 20 });
        }

        [Test]
        public void Should_bind_to_hashset_of_hashsets_of_primitives()
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
            var binder = Container.GetInstance<ISettingsBinder<HashSet<HashSet<int>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new HashSet<HashSet<int>> { new HashSet<int> { 10 }, new HashSet<int> { 12 } });
        }
    }
}