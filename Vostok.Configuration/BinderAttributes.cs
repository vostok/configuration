using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute for class which changes fields and properties behavior to required. Not required by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RequiredByDefaultAttribute : Attribute { }

    /// <inheritdoc />
    /// <summary>
    /// Attribute for required class fields and properties. It must exist in settings and not be null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredAttribute : Attribute { }

    /// <inheritdoc />
    /// <summary>
    /// Attribute for fields and properties that can be absent or be null. Is default for all fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OptionalAttribute : Attribute { }


    internal enum BinderAttribute
    {
        IsRequired = 1,
        IsOptional = 2,
    }

    internal static class AttributesExtensions
    {
        public static BinderAttribute GetAttributes(this IEnumerable<Attribute> attributes, BinderAttribute defaultAttribute)
        {
            var attrsDict = new Dictionary<Type, BinderAttribute>
            {
                { typeof(RequiredAttribute), BinderAttribute.IsRequired },
                { typeof(OptionalAttribute), BinderAttribute.IsOptional },
            };
            var attrs = attributes as Attribute[] ?? attributes.ToArray();
            if (attributes != null && attrs.Any())
                foreach (var attribute in attrs)
                    if (attrsDict.ContainsKey(attribute.GetType()))
                        return attrsDict[attribute.GetType()];
            return defaultAttribute;
        }
    }
}