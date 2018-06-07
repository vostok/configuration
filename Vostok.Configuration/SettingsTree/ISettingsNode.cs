using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration.SettingsTree
{
    public interface ISettingsNode
    {
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Current value
        /// </summary>
        [CanBeNull]
        string Value { get; }

        /// <summary>
        /// Indexed child nodes for arrays, lists
        /// </summary>
        [NotNull]
        IEnumerable<ISettingsNode> Children { get; }

        /// <summary>
        /// Named child nodes for dictionaries, fields/properties
        /// </summary>
        /// <param name="name">Key name</param>
        [CanBeNull]
        ISettingsNode this[string name] { get; }
    }
}