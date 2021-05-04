using System;
using System.Linq.Expressions;

namespace Vostok.Configuration.Validation.Constraints
{
    internal static class ConstraintExpressionCombiner
    {
        public static Expression<Func<T1, T3>> Combine<T1, T2, T3>(
            Expression<Func<T1, T2>> expression1,
            Expression<Func<T2, T3>> expression2)
        {
            var swap = new SwapVisitor(expression2.Parameters[0], expression1.Body);

            return Expression.Lambda<Func<T1, T3>>(swap.Visit(expression2.Body), expression1.Parameters);
        }

        #region SwapVisitor

        private class SwapVisitor : ExpressionVisitor
        {
            private readonly Expression from, to;

            public SwapVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }

            public override Expression Visit(Expression node)
            {
                return node == from ? to : base.Visit(node);
            }
        }

        #endregion
    }
}