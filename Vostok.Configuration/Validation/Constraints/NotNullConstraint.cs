using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class NotNullConstraint<T> : Constraint<T>
    {
        private static readonly Expression<Func<object, bool>> expression = value => value != null;

        public NotNullConstraint(Expression<Func<T, object>> fieldSelector)
            : base(ConstraintExpressionCombiner.Combine(fieldSelector, expression))
        {
        }
    }
}