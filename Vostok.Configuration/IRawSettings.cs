using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration
{
    public interface IRawSettings
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
        IEnumerable<IRawSettings> Children { get; }

        /// <summary>
        /// Named child nodes for dictionaries, fields/properties
        /// </summary>
        /// <param name="name">Key name</param>
        [CanBeNull]
        IRawSettings this[string name] { get; }
    }
}