using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class GreaterOrEqualConstraint<T, TField> : Constraint<T>
        where TField : IComparable<TField>
    {
        public GreaterOrEqualConstraint(Expression<Func<T, TField>> leftSelector, Expression<Func<T, TField>> rightSelector)
            : base(CreateExpression(leftSelector, rightSelector), CreateErrorMessage(leftSelector, rightSelector))
        {
        }

        private static Expression<Func<T, bool>> CreateExpression(Expression<Func<T, TField>> leftSelector, Expression<Func<T, TField>> rightSelector)
        {
            return config => leftSelector.Compile()(config).CompareTo(rightSelector.Compile()(config)) >= 0;
        }

        private static string CreateErrorMessage(Expression<Func<T, TField>> leftSelector, Expression<Func<T, TField>> rightSelector)
        {
            return $"Value of field '{leftSelector}' is not greater or equal compared to value of field '{rightSelector}'.";
        }
    }
}