using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Abstractions
{
    /// <inheritdoc />
    /// <summary>
    /// Sets all fields and properties required. All values must be not null. Not required by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RequiredByDefaultAttribute : Attribute { }

    /// <inheritdoc />
    /// <summary>
    /// Sets current field or property required. Value must be not null. Not required by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredAttribute : Attribute { }

    /// <inheritdoc />
    /// <summary>
    /// Sets all fields and properties optional. If value cannot be parsed it sets to default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OptionalAttribute : Attribute { }

    /// <inheritdoc />
    /// <summary>
    /// Allows validate classes and structs with specified validator
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ValidateByAttribute : Attribute
    {
        private readonly Type validatorType;

        public ValidateByAttribute(Type validatorType) => this.validatorType = validatorType;

        [NotNull]
        public object Validator => Activator.CreateInstance(validatorType);
    }
}