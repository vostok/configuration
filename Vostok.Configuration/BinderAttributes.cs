using System;

namespace Vostok.Configuration
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BaseBinderAttribute : Attribute { }

    /// <summary>
    /// Attribute for required class fields and properties. Is default for all fields and properties.
    /// </summary>
    public class RequiredAttribute : BaseBinderAttribute { }

    /// <summary>
    /// Attribute for fields and properties that can absent
    /// </summary>
    public class OptionalAttribute : BaseBinderAttribute { }

    /*public class ValidateByAttribute : BaseBinderAttribute
    {
        public ValidateByAttribute(IValidator validator)
        {
        }
    }*/
}