using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class ArrayBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Array_of_primitives()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("TRUE"),
                new ValueNode("false"),
            });
            var binder = Container.GetInstance<ISettingsBinder<bool[]>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[] { true, false });
        }

        [Test]
        public void Should_bind_to_Array_of_structs()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["Int"] = new ValueNode("1"),
                    ["String"] = new ValueNode("str1"),
                }),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["Int"] = new ValueNode("2"),
                    ["String"] = new ValueNode("str2"),
                }),
            });
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct[]>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[]
            {
                new SimpleStruct{ Int = 1, String = "str1" },
                new SimpleStruct{ Int = 2, String = "str2" },
            });
        }

        [Test]
        public void Should_throw_exception_if_tree_is_null()
        {
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct[]>>();
            new Action(() => binder.Bind(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_exception_if_tree_is_empty()
        {
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct[]>>();
            new Action(() => binder.Bind(new ArrayNode(name:null))).Should().Throw<ArgumentNullException>();
        }

        private struct SimpleStruct
        {
            public int Int { get; set; }
            public string String { get; set; }
        }
    }
}