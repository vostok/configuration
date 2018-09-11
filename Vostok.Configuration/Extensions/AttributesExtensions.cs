using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Extensions
{
    internal static class AttributesExtensions
    {
        /// <summary>
        /// Gets required or optional attribute from list of <paramref name="attributes"/>. Sets <paramref name="defaultAttribute"/> if not found.
        /// </summary>
        /// <param name="attributes">List of attributes to look in.</param>
        /// <param name="defaultAttribute">Default attribute if not found in <paramref name="attributes"/></param>
        public static BinderAttribute GetBinderAttribute(this IEnumerable<Attribute> attributes, BinderAttribute defaultAttribute)
        {
            var attrsDict = new Dictionary<Type, BinderAttribute>
            {
                {typeof(RequiredAttribute), BinderAttribute.IsRequired},
                {typeof(OptionalAttribute), BinderAttribute.IsOptional},
            };
            var attrs = attributes as Attribute[] ?? attributes.ToArray();
            foreach (var attribute in attrs)
                if (attrsDict.ContainsKey(attribute.GetType()))
                    return attrsDict[attribute.GetType()];
            return defaultAttribute;
        }
    }
}