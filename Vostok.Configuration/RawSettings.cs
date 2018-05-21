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
        /// <summary>
        /// Checks <see cref="settings"/>. Throws exeption if something is wrong.
        /// </summary>
        /// <param name="settings">Settings you're going to check</param>
        /// <param name="checkValues">Check inner values</param>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> is null</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> fields values are null and <paramref name="checkValues"/> is true</exception>
        public static void CheckSettings(RawSettings settings, bool checkValues = true)
        {
            if (settings == null)
                throw new ArgumentNullException($"Parameter \"{nameof(settings)}\" is null");
            if (checkValues && settings.IsEmpty())
                throw new ArgumentNullException($"Parameter \"{nameof(settings)}\" is empty");
        }

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with <paramref name="value"/>
        /// </summary>
        public RawSettings(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with parameters <paramref name="childrenByKey"/> and <paramref name="value"/>
        /// </summary>
        public RawSettings(IReadOnlyDictionary<string, RawSettings> childrenByKey, string value = null)
        {
            ChildrenByKey = childrenByKey;
            Value = value;
        }

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with parameters <paramref name="children"/> and <paramref name="value"/>
        /// </summary>
        public RawSettings(IReadOnlyList<RawSettings> children, string value = null)
        {
            Children = children;
            Value = value;
        }

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with parameters <paramref name="childrenByKey"/>, <paramref name="children"/>, and <paramref name="value"/>
        /// </summary>
        public RawSettings(IReadOnlyDictionary<string, RawSettings> childrenByKey, IReadOnlyList<RawSettings> children, string value = null)
        {
            ChildrenByKey = childrenByKey;
            Children = children;
            Value = value;
        }

        /// <summary>
        /// Current value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Inner values where order has no matter (dictionary, fields/properties)
        /// </summary>
        public IReadOnlyDictionary<string, RawSettings> ChildrenByKey { get; }

        /// <summary>
        /// Inner values where order has matter (array, list)
        /// </summary>
        public IReadOnlyList<RawSettings> Children { get; }

        /// <summary>
        /// Checks if fields values are null
        /// </summary>
        public bool IsEmpty() =>
            Value == null && Children == null && ChildrenByKey == null;

        #region Equality

        public override bool Equals(object obj) => Equals(obj as RawSettings);

        public bool Equals(RawSettings other)
        {
            if (other == null)
                return false;

            var thisCbkExists = ChildrenByKey != null;
            var otherCbkExists = other.ChildrenByKey != null;
            var thisChExists = Children != null;
            var otherChExists = other.Children != null;

            if (Value != other.Value ||
                thisCbkExists != otherCbkExists ||
                thisChExists != otherChExists)
                return false;

            if (thisCbkExists &&
                (!new HashSet<string>(ChildrenByKey.Keys).SetEquals(other.ChildrenByKey.Keys) ||
                 ChildrenByKey.Any(pair => !Equals(pair.Value, other.ChildrenByKey[pair.Key]))))
                return false;

            if (thisChExists && !Children.SequenceEqual(other.Children))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Value != null ? Value.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (ChildrenByKey != null ? ChildrenByKeyHash() : 0);
                hashCode = (hashCode * 397) ^ (Children != null ? ChildrenHash() : 0);
                return hashCode;
            }

            int ChildrenByKeyHash()
            {
                var keysRes = ChildrenByKey.Keys
                    .Select(k => k.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                var valsRes = ChildrenByKey.Values
                    .Select(v => v.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                return unchecked (keysRes * 195) ^ valsRes;
            }

            int ChildrenHash() =>
                Children.Select(v => v.GetHashCode()).Aggregate(0, (a, b) => unchecked(a + b));
        }

        #endregion
    }
}