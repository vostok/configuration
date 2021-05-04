using System;
using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    internal static class ConstraintAssertions
    {
        public static void ShouldPassOn<T>(this Constraint<T> constraint, T config)
        {
            var result = constraint.Check(config);

            if (!result)
                Console.Out.WriteLine(constraint.GetErrorMessage());

            Assert.True(result);
        }

        public static void ShouldFailOn<T>(this Constraint<T> constraint, T config)
        {
            var result = constraint.Check(config);

            if (!result)
                Console.Out.WriteLine(constraint.GetErrorMessage());

            Assert.False(result);
        }
    }
}