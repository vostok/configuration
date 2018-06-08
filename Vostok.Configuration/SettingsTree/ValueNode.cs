using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.SettingsTree
{
    internal sealed class ValueNode : ISettingsNode
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
    }
}