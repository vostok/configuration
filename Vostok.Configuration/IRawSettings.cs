using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration
{
    /// <summary>
    /// Represents a tree of raw settings. 'Raw' means that all values are stored as strings.
    /// </summary>
    public interface IRawSettings
    {
        /// <summary>
        /// Name of the tree node.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Value of the tree node. Not null for leaf nodes only.
        /// </summary>
        [CanBeNull]
        string Value { get; }

        /// <summary>
        /// A view of child nodes as an ordered collection. Used for nodes that represent lists or arrays.
        /// </summary>
        [NotNull]
        IEnumerable<IRawSettings> Children { get; }

        /// <summary>
        /// A view of child nodes as an indexed collection. Used for nodes that represent dictionaries or classes.
        /// </summary>
        [CanBeNull]
        IRawSettings this[string name] { get; }
    }
}