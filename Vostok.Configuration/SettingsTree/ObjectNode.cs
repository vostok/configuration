using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.SettingsTree
{
    internal sealed class ObjectNode : ISettingsNode //, IEquatable<RawSettings>
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

        #region Equality

        /*public override bool Equals(object obj) => Equals(obj as RawSettings);

        public bool Equals(RawSettings other)
        {
            if (other == null)
                return false;

            var thisChExists = children != null;
            var otherChExists = other.children != null;

            if (Value != other.Value ||
                Name != other.Name ||
                thisChExists != otherChExists)
                return false;

            if (thisChExists &&
                (!new HashSet<object>(children.Keys.Cast<object>()).SetEquals(other.children.Keys.Cast<object>()) ||
                 !new HashSet<RawSettings>(children.Values.Cast<RawSettings>()).SetEquals(other.children.Values.Cast<RawSettings>())))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var valueHashCode = Value != null ? Value.GetHashCode() : 0;
                var nameHashCode = Name.GetHashCode();
                var hashCode = ((valueHashCode + nameHashCode) * 397) ^ (children != null ? ChildrenHash() : 0);
                return hashCode;
            }

            int ChildrenHash()
            {
                var keysRes = children.Keys.OfType<string>()
                    .Select(k => k.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                var valsRes = children.Values.Cast<RawSettings>()
                    .Select(v => v.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                return unchecked (keysRes * 195) ^ valsRes;
            }
        }*/

        #endregion
    }
}