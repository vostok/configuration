using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class NotNullOrEmptyConstraint<T> : Constraint<T>
    {
        public NotNullOrEmptyConstraint(Expression<Func<T, string>> fieldSelector)
            : base(ConstraintExpressionCombiner.Combine(fieldSelector, expression))
        {
        }

        private static readonly Expression<Func<string, bool>> expression = value => !string.IsNullOrEmpty(value);
    }
}