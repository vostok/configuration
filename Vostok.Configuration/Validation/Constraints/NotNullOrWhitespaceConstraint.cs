using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class NotNullOrWhitespaceConstraint<T> : Constraint<T>
    {
        private static readonly Expression<Func<string, bool>> expression = value => !string.IsNullOrWhiteSpace(value);

        public NotNullOrWhitespaceConstraint(Expression<Func<T, string>> fieldSelector)
            : base(ConstraintExpressionCombiner.Combine(fieldSelector, expression))
        {
        }
    }
}