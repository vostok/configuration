using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.SettingsTree.Mutable
{
    internal sealed class UniversalNode : ISettingsNode
    {
        private readonly IList<ISettingsNode> childrenList = new List<ISettingsNode>();
        private readonly IDictionary<string, ISettingsNode> childrenDict = new SortedDictionary<string, ISettingsNode>(StringComparer.InvariantCultureIgnoreCase);

        public UniversalNode(string value, string name = null)
        {
            Value = value;
            Name = name ?? value;
        }

        public UniversalNode(IList<ISettingsNode> children, string name = null)
        {
            childrenList = children;
            Name = name;
        }

        public UniversalNode(IDictionary<string, ISettingsNode> children, string name = null)
        {
            childrenDict = children;
            Name = name;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public IEnumerable<ISettingsNode> ChildrenList => childrenList.AsEnumerable();
        public IEnumerable<ISettingsNode> ChildrenDict => childrenDict.Values.AsEnumerable();
        public ISettingsNode this[string name]
        {
            get
            {
                if (childrenDict.Any())
                    return childrenDict.TryGetValue(name, out var res) ? res : null;
                if (childrenList.Any())
                    return int.TryParse(name, out var index) && index >= 0 && index < childrenList.Count ? childrenList[index] : null;
                return null;
            }
        }
        IEnumerable<ISettingsNode> ISettingsNode.Children => Enumerable.Empty<UniversalNode>();
        ISettingsNode ISettingsNode.this[string name] => null;
        public ISettingsNode Merge(ISettingsNode other, SettingsMergeOptions options) => null;

        public void Add(ISettingsNode value) => childrenList.Add(value);
        public void Add(string key, ISettingsNode value) => childrenDict.Add(key, value);

        public static explicit operator ValueNode(UniversalNode settings) =>
            new ValueNode(settings.Value, settings.Name);

        public static explicit operator ArrayNode(UniversalNode settings)
        {
            IReadOnlyList<ISettingsNode> list = !settings.ChildrenList.Any()
                ? null
                : settings.ChildrenList.Select(c => ConvertNode((UniversalNode)c)).ToList();
            return new ArrayNode(list, settings.Name);
        }

        public static explicit operator ObjectNode(UniversalNode settings)
        {
            var dict = settings.ChildrenDict.ToSortedDictionary(node => node.Name, node => ConvertNode((UniversalNode)node), StringComparer.InvariantCultureIgnoreCase);
            return new ObjectNode(dict, settings.Name);
        }

        private static ISettingsNode ConvertNode(UniversalNode universalNode)
        {
            if (universalNode.ChildrenDict.Any())
                return (ObjectNode)universalNode;
            if (universalNode.childrenList.Any())
                return (ArrayNode)universalNode;
            return (ValueNode)universalNode;
        }
    }
}