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


        private bool ChildrenByKeyExists() => ChildrenByKey != null;
        private bool ChildrenExists() => Children != null;

        /// <summary>
        /// Current value
        /// </summary>
        public string Value { get; }

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
                if (!ChildrenByKey.Keys.All(k => other.ChildrenByKey.Keys.Contains(k)) ||
                    !other.ChildrenByKey.Keys.All(k => ChildrenByKey.Keys.Contains(k)))
                    return false;
                foreach (var pair in ChildrenByKey)
                    if (!Equals(pair.Value, other.ChildrenByKey[pair.Key]))
                        return false;
            }

            if (ChildrenExists())
            {
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