using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration
{
    /// <summary>
    /// Tree of settings
    /// </summary>
    public sealed class RawSettings : IEquatable<RawSettings>
    {
        public RawSettings() { }

        public RawSettings(string value)
        {
            Value = value;
        }

        public RawSettings(IReadOnlyDictionary<string, RawSettings> children, string value = null)
        {
            ChildrenByKey = children;
            Value = value;
        }

        public RawSettings(IReadOnlyList<RawSettings> children, string value = null)
        {
            Children = children;
            Value = value;
        }

        public RawSettings(IReadOnlyDictionary<string, RawSettings> childrenByKey, IReadOnlyList<RawSettings> children, string value = null)
        {
            ChildrenByKey = childrenByKey;
            Children = children;
            Value = value;
        }

        // CR(krait): RawSettings is immutable, these two methods should not exist.
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

        // CR(krait): It doesn't look like these methods improve readability. The null check is pretty obvious by itself, and they don't even make it shorter.
        private bool ChildrenByKeyExists() => ChildrenByKey != null;
        private bool ChildrenExists() => Children != null;

        /// <summary>
        /// Current value
        /// </summary>
        public string Value { get; }
        
        // CR(krait): These properties should not have setters.
        /// <summary>
        /// Inner values where order has no matter (dictioonary, fields/properties)
        /// </summary>
        public IReadOnlyDictionary<string, RawSettings> ChildrenByKey { get; private set; }

        /// <summary>
        /// Inner values where order has matter (array, list)
        /// </summary>
        public IReadOnlyList<RawSettings> Children { get; private set; }

        #region Equality

        public override bool Equals(object obj) => Equals(obj as RawSettings);

        public bool Equals(RawSettings other)
        {
            if (other == null)
                return false;

            if (Value != other.Value ||
                ChildrenByKeyExists() != other.ChildrenByKeyExists() ||
                ChildrenExists() != other.ChildrenExists())
                return false;

            if (ChildrenByKeyExists())
            {
                // CR(krait): new HashSet<string>(ChildrenByKey.Keys).SetEquals(other.ChildrenByKey.Keys) is faster and more readable.
                if (!ChildrenByKey.Keys.All(k => other.ChildrenByKey.Keys.Contains(k)) ||
                    !other.ChildrenByKey.Keys.All(k => ChildrenByKey.Keys.Contains(k)))
                    return false;
                // CR(krait): But here .All() would look nice.
                foreach (var pair in ChildrenByKey)
                    if (!Equals(pair.Value, other.ChildrenByKey[pair.Key]))
                        return false;
            }

            if (ChildrenExists())
            {
                // CR(krait): .SequenceEqual()
                if (Children.Count != other.Children.Count)
                    return false;
                if (Children.Where((t, i) => !Equals(t, other.Children[i])).Any())
                    return false;
            }

            return true;
        }

        // CR(krait): It'd be better to override GetHashCode too, to avoid nasty surprises.
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}