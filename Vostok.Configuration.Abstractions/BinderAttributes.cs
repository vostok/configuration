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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ValidateByAttribute : Attribute
    {
        private readonly Type validatorType;

        public ValidateByAttribute(Type validatorType) =>
            this.validatorType = validatorType;

        [NotNull]
        public ISettingsValidator<T> CastValidator<T>()
        {
            if (!(Activator.CreateInstance(validatorType) is ISettingsValidator<T> validator))
                throw new SettingsValidationException($"A validator of type '{validatorType}' cannot be used to validate an instance of type '{typeof(T)}'.");

            return validator;
        }
    }
}