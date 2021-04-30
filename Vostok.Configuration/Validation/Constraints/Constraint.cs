using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Vostok.Configuration.Validation.Constraints
{
    [PublicAPI]
    public class Constraint<T>
    {
        public Constraint(Expression<Func<T, bool>> rule, string errorMessage = "")
        {
            this.rule = rule;
            this.errorMessage = errorMessage;
        }

        public bool Check(T item)
        {
            return rule.Compile()(item);
        }

		public string GetErrorMessage()
		{
			return $"Constraint violated: '{(string.IsNullOrEmpty(errorMessage) ? rule.ToString() : errorMessage)}'.";
		}

        private readonly Expression<Func<T, bool>> rule;
        private readonly string errorMessage;
    }
}