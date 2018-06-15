using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.MergeOptions;

namespace Vostok.Configuration.SettingsTree
{
    public sealed class ValueNode : ISettingsNode, IEquatable<ValueNode>
    {
        public ValueNode(string value, string name = null)
        {
            Value = value;
            Name = name ?? value;
        }

        public string Name { get; }
        public string Value { get; }
        IEnumerable<ISettingsNode> ISettingsNode.Children => Enumerable.Empty<ArrayNode>();
        ISettingsNode ISettingsNode.this[string name] => null;

        public ISettingsNode Merge(ISettingsNode other, SettingsMergeOptions options = null) => other;

        #region Equality

        public override bool Equals(object obj) => Equals(obj as ValueNode);

        public bool Equals(ValueNode other) =>
            other != null && Value == other.Value && Name == other.Name;

        public override int GetHashCode()
        {
            unchecked
            {
                var valueHashCode = Value?.GetHashCode() ?? 0;
                var nameHashCode = Name?.GetHashCode() ?? 0;
                return valueHashCode * 397 + nameHashCode;
            }
        }

        #endregion
    }
}