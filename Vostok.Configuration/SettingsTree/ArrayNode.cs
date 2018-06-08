using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.SettingsTree
{
    internal sealed class ArrayNode : ISettingsNode
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
    }
}