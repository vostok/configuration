using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.SettingsTree
{
    [TestFixture]
    public class ObjectNode_Tests
    {
        [Test]
        public void Constructors_are_equal()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") }, "Name");
            var sets2 = new ObjectNode("Name", new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") });
            sets1.Should().BeEquivalentTo(sets2);
            Equals(sets1, sets2).Should().BeTrue("checks overrided Equals method");
        }

        [Test]
        public void Equals_returns_false_by_name()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") }, "Name1");
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") }, "Name2");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_children_key()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value1"] = new ValueNode("x") }, "Name");
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value2"] = new ValueNode("x") }, "Name");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_children_value()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x1") }, "Name");
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x2") }, "Name");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") }, "Name").GetHashCode();
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode> { ["value"] = new ValueNode("x") }, "Name").GetHashCode();
            sets1.Should().Be(sets2);
        }

        [Test]
        public void Keys_should_be_case_insensitive()
        {
            var sets = new ObjectNode(new SortedDictionary<string, ISettingsNode>(new ChildrenKeysComparer())
            {
                ["value"] = new ValueNode("v0"),
                ["VALUE"] = new ValueNode("v1"),    //rewrites
                ["TeSt"] = new ValueNode("v2"),
            }, "Name");
            
            sets.Children.Count().Should().Be(2, "v0 was rewrited");

            sets["value"].Value.Should().Be("v1");
            sets["VALUE"].Value.Should().Be("v1");
            sets["TEST"].Value.Should().Be("v2");
            sets["test"].Value.Should().Be("v2");
        }

        [Test]
        public void Should_return_second_tree_on_shallow_merge_and_same_children_names()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["value1"] = new ValueNode("x1", "value1"),
                ["value2"] = new ValueNode("x1", "value2"),
            });
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["value1"] = new ValueNode("x2", "value1"),
                ["value3"] = new ValueNode("x2", "value3"),
            });

            var merge = sets1.Merge(sets2, new SettingsMergeOptions {TreeMergeStyle = TreeMergeStyle.Shallow});
            merge.Should().Be(sets2);
        }

        [Test]
        public void Should_return_other_on_shallow_merge_and_same_children_names()
        {
            var comparer = new ChildrenKeysComparer();
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
            {
                ["value1"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
                {
                    ["subvalue"] = new ValueNode("sx1", "subvalue"),
                }, "value1"),
                ["value2"] = new ValueNode("x1", "value2"),
            });
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
            {
                ["value2"] = new ValueNode("x2", "value2"),
                ["VALUE1"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
                {
                    ["subvalue"] = new ValueNode("sx2", "subvalue"),
                }, "VALUE1"),
            });

            var merge = sets1.Merge(sets2, new SettingsMergeOptions {TreeMergeStyle = TreeMergeStyle.Shallow});
            merge["value2"].Value.Should().Be("x2");
            merge["value2"].Name.Should().Be("value2");
            merge["value1"].Name.Should().Be("VALUE1");
            merge["value1"]["subvalue"].Value.Should().Be("sx2");
            merge["value1"]["subvalue"].Name.Should().Be("subvalue");
        }

        [Test]
        public void Should_return_other_on_another_node_type()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode>{ ["value"] = new ValueNode("x1") });
            var sets2 = new ValueNode("x2");

            var merge = sets1.Merge(sets2, null);
            merge.Value.Should().Be("x2");

            merge = sets2.Merge(sets1, null);
            merge["value"].Value.Should().Be("x1");
        }

        [Test]
        public void Should_make_deep_merge_correctly()
        {
            var comparer = new ChildrenKeysComparer();
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
            {
                ["value1"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
                {
                    ["sv1"] = new ValueNode("sx1", "sv1"),
                    ["sv2"] = new ValueNode("sx1", "sv2"),
                }, "value1"),
                ["value2"] = new ValueNode("x1", "value2"),
                ["value3"] = new ValueNode("x1", "value3"),
            });
            var sets2 = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
            {
                ["VALUE1"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>(comparer)
                {
                    ["SV2"] = new ValueNode("sx2", "sv2"),
                    ["sv3"] = new ValueNode("sx2", "sv3"),
                }, "VALUE1"),
                ["VALUE2"] = new ValueNode("x2", "VALUE2"),
            });

            var merge = sets1.Merge(sets2, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Deep });
            merge["value1"]["sv1"].Value.Should().Be("sx1");
            merge["value1"]["sv2"].Value.Should().Be("sx2");
            merge["value1"]["sv3"].Value.Should().Be("sx2");
            merge["value2"].Value.Should().Be("x2");
            merge["value3"].Value.Should().Be("x1");
        }
    }
}