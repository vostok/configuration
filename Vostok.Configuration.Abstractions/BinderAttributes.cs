using System;

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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ValidateByAttribute : Attribute
    {
        private readonly Type iValidatorType;

        public ValidateByAttribute(Type iValidatorType) =>
            this.iValidatorType = iValidatorType;

        public IValidator Validator => Activator.CreateInstance(iValidatorType) as IValidator;
    }
}