using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.SettingsTree
{
    [TestFixture]
    public class ValueNode_Tests
    {
        [Test]
        public void Name_should_equal_value_if_null()
        {
            var sets = new ValueNode("x");
            sets.Name.Should().Be("x");
        }

        [Test]
        public void Equals_returns_false_by_name()
        {
            var sets1 = new ValueNode("x", "name1");
            var sets2 = new ValueNode("x", "name2");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_value()
        {
            var sets1 = new ValueNode("x1", "name");
            var sets2 = new ValueNode("x2", "name");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = new ValueNode("x", "name").GetHashCode();
            var sets2 = new ValueNode("x", "name").GetHashCode();
            sets1.Should().Be(sets2);
        }

        [Test]
        public void Should_always_return_second_node_on_merge()
        {
            var sets1 = new ValueNode("x", "name1");
            var sets2 = new ValueNode("x", "name2");
            var merge = sets1.Merge(sets2, null);
            merge.Value.Should().Be("x");
            merge.Name.Should().Be("name2");

            sets1 = new ValueNode("x1", "name");
            sets2 = new ValueNode("x2", "name");
            merge = sets1.Merge(sets2, null);

            merge.Value.Should().Be("x2");
            merge.Name.Should().Be("name");
        }
    }
}