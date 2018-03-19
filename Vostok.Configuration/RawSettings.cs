using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration
{
    /// <summary>
    /// Tree of settings
    /// </summary>
    public sealed class RawSettings
    {
        public RawSettings() { }

        public RawSettings(string value)
        {
            Value = value;
        }

        public RawSettings(IDictionary<string, RawSettings> children, string value = null)
        {
            ChildrenByKey = children;
            Value = value;
        }
        public RawSettings(IList<RawSettings> children, string value = null)
        {
            Children = children;
            Value = value;
        }
        public RawSettings(IDictionary<string, RawSettings> childrenByKey, IList<RawSettings> children, string value = null)
        {
            ChildrenByKey = childrenByKey;
            Children = children;
            Value = value;
        }

        /// <summary>
        /// Creates ChildrenByKey dictionary
        /// </summary>
        public void CreateDictionary()
        {
            ChildrenByKey = new Dictionary<string, RawSettings>();
        }

        /// <summary>
        /// Creates Children list
        /// </summary>
        public void CreateList()
        {
            Children = new List<RawSettings>();
        }

        /// <summary>
        /// Compares one RawSettings tree to another
        /// </summary>
        /// <param name="first">First RawSettings tree</param>
        /// <param name="second">Second RawSettings tree</param>
        /// <returns>Comparison result</returns>
        public static bool Equals(RawSettings first, RawSettings second)
        {
            if (first == null && second == null)
                return true;
            else if (first == null || second == null)
                return false;

            if (first.Value != second.Value ||
                first.ChildrenByKeyExists() != second.ChildrenByKeyExists() ||
                first.ChildrenExists() != second.ChildrenExists())
                return false;

            if (first.ChildrenByKeyExists())
            {
                if (!first.ChildrenByKey.Keys.SequenceEqual(second.ChildrenByKey.Keys))
                    return false;
                foreach (var pair in first.ChildrenByKey)
                    if (!Equals(pair.Value, second.ChildrenByKey[pair.Key]))
                        return false;
            }

            if (first.ChildrenExists())
            {
                if (first.Children.Count != second.Children.Count)
                    return false;
                if (first.Children.Where((t, i) => !Equals(t, second.Children[i])).Any())
                    return false;
            }

            return true;
        }

        private bool ChildrenByKeyExists() => ChildrenByKey != null;
        private bool ChildrenExists() => Children != null;

        /// <summary>
        /// Current value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Inner values where order has no matter (dictioonary, fields/properties)
        /// </summary>
        public IDictionary<string, RawSettings> ChildrenByKey { get; private set; }

        /// <summary>
        /// Inner values where order has matter (array, list)
        /// </summary>
        public IList<RawSettings> Children { get; private set; }
    }

    // TODO(krait): validator (+custom specified by attribute), example generator (+config saving)
    // TODO(krait): attributes list
}