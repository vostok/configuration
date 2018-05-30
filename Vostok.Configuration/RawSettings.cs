﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Vostok.Configuration
{
    /// <inheritdoc cref="IRawSettings" />
    /// <summary>
    /// Tree of settings
    /// </summary>
    public sealed class RawSettings : IRawSettings, IEquatable<RawSettings>
    {
        /// <summary>
        /// Checks <see cref="settings"/>. Throws exeption if something is wrong.
        /// </summary>
        /// <param name="settings">Settings you're going to check</param>
        /// <param name="checkValues">Check inner values</param>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> is null</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> fields values are null and <paramref name="checkValues"/> is true</exception>
        public static void CheckSettings(IRawSettings settings, bool checkValues = true)
        {
            if (settings == null)
                throw new ArgumentNullException($"Parameter \"{nameof(settings)}\" is null");
            if (checkValues && settings.Value == null && !settings.Children.Any())
                throw new ArgumentNullException($"Parameter \"{nameof(settings)}\" is empty");
        }

        /// <summary>
        /// Child nodes. Key for dictionaries, fields/properties, indexes for arrays, lists
        /// </summary>
        private readonly IOrderedDictionary children = new OrderedDictionary();

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with <paramref name="value"/>
        /// </summary>
        public RawSettings(string value, string name = "")
        {
            Value = value;
            Name = name == "" ? value : name;
        }

        /// <summary>
        /// Creates <see cref="RawSettings"/> instance with parameters <paramref name="children"/> and <paramref name="value"/>
        /// </summary>
        public RawSettings(IOrderedDictionary children, string name = "", string value = null)
        {
            this.children = children;
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
        public IRawSettings this[string name] => children[name] as IRawSettings;
        //todo
        public IEnumerable<IRawSettings> Children =>
            children?.Values.Cast<RawSettings>() ?? Enumerable.Empty<RawSettings>();
        
        #region Equality

        public override bool Equals(object obj) => Equals(obj as RawSettings);

        public bool Equals(RawSettings other)
        {
            if (other == null)
                return false;

            var thisChExists = children != null;
            var otherChExists = other.children != null;

            if (Value != other.Value ||
                Name != other.Name ||
                thisChExists != otherChExists)
                return false;

            if (thisChExists &&
                (!new HashSet<object>(children.Keys.Cast<object>()).SetEquals(other.children.Keys.Cast<object>()) ||
                 !new HashSet<RawSettings>(children.Values.Cast<RawSettings>()).SetEquals(other.children.Values.Cast<RawSettings>())))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var valueHashCode = Value != null ? Value.GetHashCode() : 0;
                var nameHashCode = Name.GetHashCode();
                var hashCode = ((valueHashCode + nameHashCode) * 397) ^ (children != null ? ChildrenHash() : 0);
                return hashCode;
            }

            int ChildrenHash()
            {
                var keysRes = children.Keys.OfType<string>()
                    .Select(k => k.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                var valsRes = children.Values.Cast<RawSettings>()
                    .Select(v => v.GetHashCode())
                    .Aggregate(0, (a, b) => unchecked(a + b));
                return unchecked (keysRes * 195) ^ valsRes;
            }
        }

        #endregion
    }
}