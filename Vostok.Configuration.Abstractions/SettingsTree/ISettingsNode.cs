using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.MergeOptions;

namespace Vostok.Configuration.Abstractions.SettingsTree
{
    /// <summary>
    /// Represents a tree of raw settings. 'Raw' means that all values are stored as strings.
    /// </summary>
    public interface ISettingsNode
    {
        /// <summary>
        /// Name of the tree node. Null for array elements.
        /// </summary>
        [CanBeNull]
        string Name { get; }

        /// <summary>
        /// Value of the tree node. Not null for leaf nodes only.
        /// </summary>
        [CanBeNull]
        string Value { get; }

        /// <summary>
        /// A view of child nodes as an ordered collection. The order is same as in the source.
        /// </summary>
        [NotNull]
        IEnumerable<ISettingsNode> Children { get; }

        /// <summary>
        /// A view of child nodes as a collection indexed by node names. Used for nodes that represent dictionaries or classes. Array elements cannot be accessed this way.
        /// </summary>
        [CanBeNull]
        ISettingsNode this[string name] { get; }

        /// <summary>
        /// Merge two settings trees by rules specified in <paramref name="options"/>.
        /// </summary>
        ISettingsNode Merge(ISettingsNode other, SettingsMergeOptions options = null);
    }
}