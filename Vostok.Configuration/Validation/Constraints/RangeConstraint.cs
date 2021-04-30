using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class RangeConstraint<T, TField> : Constraint<T>
        where TField : IComparable<TField>
    {
        public RangeConstraint(Expression<Func<T, TField>> fieldSelector, TField from, TField to, bool inclusive = true)
            : base (ConstraintExpressionCombiner.Combine(fieldSelector, CreateExpression(from, to, inclusive)), CreateErrorMessage(fieldSelector, from, to, inclusive))
        {
        }

        private static Expression<Func<TField, bool>> CreateExpression(TField from, TField to, bool inclusive)
        {
            if (inclusive)
                return field => field.CompareTo(from) >= 0 && field.CompareTo(to) <= 0;

            return field => field.CompareTo(from) > 0 && field.CompareTo(to) < 0;
        }

        private static string CreateErrorMessage(Expression<Func<T, TField>> fieldSelector, TField from, TField to, bool inclusive)
        {
            return inclusive
                ? $"Value of field '{fieldSelector}' is out of allowed range [{FormatBoundary(@from)}; {FormatBoundary(to)}]."
                : $"Value of field '{fieldSelector}' is out of allowed range ({FormatBoundary(@from)}; {FormatBoundary(to)}).";
        }

        private static string FormatBoundary(object boundary)
        {
            if (boundary is TimeSpan span)
                return  span.ToPrettyString();

            return boundary.ToString();
        }
    }
}