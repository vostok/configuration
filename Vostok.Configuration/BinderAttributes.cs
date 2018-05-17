using System;

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
}