using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Validation
{
    [PublicAPI]
    public abstract class ConstraintsValidator<TSettings> : ISettingsValidator<TSettings>
    {
        public IEnumerable<string> Validate(TSettings settings)
            => GetConstraints()
                .Where(constraint => !constraint.Check(settings))
                .Select(constraint => constraint.GetErrorMessage());

        protected abstract IEnumerable<Constraint<TSettings>> GetConstraints();
    }
}