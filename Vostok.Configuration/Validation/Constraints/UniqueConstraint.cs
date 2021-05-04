using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class UniqueConstraint<T, TField> : Constraint<T>
    {
        public UniqueConstraint(params Expression<Func<T, TField>>[] selectors)
            : this(EqualityComparer<TField>.Default, selectors)
        {
        }

        public UniqueConstraint(IEqualityComparer<TField> comparer, params Expression<Func<T, TField>>[] selectors)
            : base(CreateExpression(comparer, selectors), CreateErrorMessage(selectors))
        {
        }

        private static Expression<Func<T, bool>> CreateExpression(IEqualityComparer<TField> comparer, params Expression<Func<T, TField>>[] selectors)
        {
            return config => selectors.Select(selector => selector.Compile()(config)).Distinct(comparer).Count() == selectors.Length;
        }

        private static string CreateErrorMessage(params Expression<Func<T, TField>>[] selectors)
        {
            return string.Format("Following fields set has some non unique values: " + string.Join(", ", selectors.Select(s => s.ToString())));
        }
    }
}