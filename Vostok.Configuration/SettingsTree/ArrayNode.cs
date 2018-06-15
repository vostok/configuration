using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.SettingsTree
{
    public sealed class ArrayNode : ISettingsNode, IEquatable<ArrayNode>
    {
        private readonly IReadOnlyList<ISettingsNode> children;

        public ArrayNode(string name, IReadOnlyList<ISettingsNode> children = null)
        {
            Name = name;
            this.children = children;
        }

        public ArrayNode(IReadOnlyList<ISettingsNode> children, string name = null)
            : this(name, children)
        {
        }

        public string Name { get; }
        public IEnumerable<ISettingsNode> Children => children.AsEnumerable() ?? Enumerable.Empty<ArrayNode>();
        public ISettingsNode this[string name] => int.TryParse(name, out var index) && index >= 0 && index < children.Count ? children[index] : null;
        string ISettingsNode.Value { get; } = null;

        public ISettingsNode Merge(ISettingsNode other, SettingsMergeOptions options = null)
        {
            if (!(other is ArrayNode))
                return other;

            if (options == null)
                options = new SettingsMergeOptions();

            switch (options.ListMergeStyle)
            {
                case ListMergeStyle.Replace:
                    return other;
                case ListMergeStyle.Concat:
                    return new ArrayNode(children.Concat(other.Children).ToList(), Name);
                case ListMergeStyle.Union:
                    return new ArrayNode(children.Union(other.Children).ToList(), Name);
                default:
                    return null;
            }
        }

        #region Equality

        public override bool Equals(object obj) => Equals(obj as ArrayNode);

        public bool Equals(ArrayNode other)
        {
            if (other == null)
                return false;

            var thisChExists = children != null;
            var otherChExists = other.children != null;

            if (Name != other.Name ||
                thisChExists != otherChExists)
                return false;

            if (thisChExists && !new HashSet<ISettingsNode>(children).SetEquals(other.children))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var nameHashCode = Name?.GetHashCode() ?? 0;
                var hashCode = (nameHashCode * 397) ^ (children != null ? ChildrenHash() : 0);
                return hashCode;
            }

            int ChildrenHash()
            {
                var res = children
                    .Select(k => k.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                return unchecked(res * 599);
            }
        }

        #endregion
    }
}