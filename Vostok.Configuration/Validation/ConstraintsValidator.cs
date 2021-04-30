using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Validation
{
    [PublicAPI]
    public static class ConstraintsValidator
    {
        /// <summary>
        /// Returns error messages only for violated constraints.
        /// </summary>
        public static IEnumerable<string> GetViolatedConstraintsErrors<TSettings>(TSettings settings, IEnumerable<Constraint<TSettings>> constraints)
        {
            foreach (var constraint in constraints)
                if (!constraint.Check(settings))
                    yield return constraint.GetErrorMessage();
        }
    }
}