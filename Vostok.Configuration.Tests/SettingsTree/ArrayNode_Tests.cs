using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.MergeOptions;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.SettingsTree
{
    [TestFixture]
    public class ArrayNode_Tests
    {
        [Test]
        public void Constructors_are_equal()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x") }, "Name");
            var sets2 = new ArrayNode("Name", new List<ISettingsNode> { new ValueNode("x") });
            sets1.Should().BeEquivalentTo(sets2);
            Equals(sets1, sets2).Should().BeTrue("checks overrided Equals method");
        }

        [Test]
        public void Equals_returns_false_by_name()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x") }, "Name1");
            var sets2 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x") }, "Name2");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_children_value()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x1") }, "Name");
            var sets2 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x2") }, "Name");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x") }, "Name").GetHashCode();
            var sets2 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x") }, "Name").GetHashCode();
            sets1.Should().Be(sets2);
        }

        [Test]
        public void Should_return_other_on_another_node_type()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode> { new ValueNode("x1") });
            var sets2 = new ValueNode("x2");

            var merge = sets1.Merge(sets2, null);
            merge.Value.Should().Be("x2");

            merge = sets2.Merge(sets1, null);
            merge["0"].Value.Should().Be("x1");
        }

        [TestCase(ListMergeStyle.Replace, TestName = "Replace option")]
        [TestCase(ListMergeStyle.Concat, TestName = "Concat option")]
        [TestCase(ListMergeStyle.Union, TestName = "Union option")]
        public void Should_merge_with_different_options(ListMergeStyle style)
        {
            var sets1 = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("x1"),
                new ValueNode("x2"),
                new ValueNode("x3"),
            });
            var sets2 = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("x1"),
                new ValueNode("x4"),
                new ValueNode("x5"),
            });

            var merge = sets1.Merge(sets2, new SettingsMergeOptions { ListMergeStyle = style });
            switch (style)
            {
                case ListMergeStyle.Replace:
                    merge.Children.Select(c => c.Value).Should().ContainInOrder("x1", "x4", "x5");
                    break;
                case ListMergeStyle.Concat:
                    merge.Children.Select(c => c.Value).Should().ContainInOrder("x1", "x2", "x3", "x1", "x4", "x5");
                    break;
                case ListMergeStyle.Union:
                    merge.Children.Select(c => c.Value).Should().ContainInOrder("x1", "x2", "x3", "x4", "x5");
                    break;
            }
        }

        [Test]
        public void Should_merge_wirh_different_options_right_way()
        {
            var sets1 = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("x1"),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["value"] = new ValueNode("x11"),
                }),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["value"] = new ValueNode("x12"),
                }),
            });
            var sets2 = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("x1"),
                new ValueNode("x2"),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["value"] = new ValueNode("x11"),
                }),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["value"] = new ValueNode("x21"),
                }),
            });

            var merge = sets1.Merge(sets2, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Shallow, ListMergeStyle = ListMergeStyle.Union });
            var children = merge.Children.ToArray();
            children[0].Value.Should().Be("x1");
            children[1]["value"].Value.Should().Be("x11");
            children[2]["value"].Value.Should().Be("x12");
            children[3].Value.Should().Be("x2");
            children[4]["value"].Value.Should().Be("x21");
        }
    }
}