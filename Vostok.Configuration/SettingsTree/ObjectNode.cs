using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.SettingsTree
{
    public sealed class ObjectNode : ISettingsNode, IEquatable<ObjectNode>
    {
        private readonly IReadOnlyDictionary<string, ISettingsNode> children;

        public ObjectNode(string name, IReadOnlyDictionary<string, ISettingsNode> children = null)
        {
            Name = name;
            this.children = children;
        }

        public ObjectNode(IReadOnlyDictionary<string, ISettingsNode> children, string name = null)
            : this(name, children)
        {
        }

        public string Name { get; }
        public IEnumerable<ISettingsNode> Children => children?.Values ?? Enumerable.Empty<ObjectNode>();
        public ISettingsNode this[string name] => children.TryGetValue(name, out var res) ? res : null;
        string ISettingsNode.Value { get; } = null;

        public ISettingsNode Merge(ISettingsNode other, SettingsMergeOptions options = null)
        {
            if (!(other is ObjectNode))
                return other;

            if (options == null)
                options = new SettingsMergeOptions();

            switch (options.TreeMergeStyle)
            {
                case TreeMergeStyle.Shallow:
                    return ShallowMerge(other, options);
                case TreeMergeStyle.Deep:
                    return DeepMerge(other, options);
                default:
                    return null;
            }
        }

        private ISettingsNode DeepMerge(ISettingsNode other, SettingsMergeOptions options)
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var thisNames = children.Keys.ToArray();
            var otherNames = other.Children.Select(c => c.Name).ToArray();
            var duplicates = otherNames.Intersect(thisNames, comparer).ToArray();
            var unique = thisNames.Unique(otherNames, comparer).ToArray();

            var dict = new SortedDictionary<string, ISettingsNode>(comparer);
            foreach (var name in unique)
                dict.Add(name, this[name] ?? other[name]);
            foreach (var name in duplicates)
                dict.Add(name, this[name].Merge(other[name], options));
            return new ObjectNode(dict, other.Name);
        }

        private ISettingsNode ShallowMerge(ISettingsNode other, SettingsMergeOptions options)
        {
            var thisNames = children.Keys.OrderBy(k => k).ToArray();
            var otherNames = other.Children.Select(c => c.Name).OrderBy(k => k).ToArray();
            if (!thisNames.SequenceEqual(otherNames, StringComparer.InvariantCultureIgnoreCase))
                return other;
            var dict = new SortedDictionary<string, ISettingsNode>();
            foreach (var name in thisNames)
                dict.Add(name, this[name].Merge(other[name], options));
            return new ObjectNode(dict, other.Name);
        }

        #region Equality

        public override bool Equals(object obj) => Equals(obj as ObjectNode);

        public bool Equals(ObjectNode other)
        {
            if (other == null)
                return false;

            var thisChExists = children != null;
            var otherChExists = other.children != null;

            if (Name != other.Name ||
                thisChExists != otherChExists)
                return false;

            if (thisChExists &&
                (!new HashSet<string>(children.Keys).SetEquals(other.children.Keys) ||
                 !new HashSet<ISettingsNode>(children.Values).SetEquals(other.children.Values)))
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
                var keysRes = children.Keys
                    .Select(k => k.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                var valsRes = children.Values
                    .Select(v => v.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                return unchecked(keysRes * 599) ^ valsRes;
            }
        }

        #endregion
    }
}