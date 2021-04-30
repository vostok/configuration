using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class RangeConstraint_Tests
    {
        [TestCase(5, 0, 10, true)]
        [TestCase(0, 0, 10, true)]
        [TestCase(10, 0, 10, true)]
        [TestCase(1, 0, 10, false)]
        [TestCase(9, 0, 10, false)]
        public void Should_pass_when_value_lies_inside_range(int value, int from, int to, bool inclusive)
        {
            var config = new TestConfig {Field = value};
            var constraint = new RangeConstraint<TestConfig, int>(c => c.Field, from, to, inclusive);

            constraint.ShouldPassOn(config);
        }

        [TestCase(int.MinValue, 0, 10, true)]
        [TestCase(int.MaxValue, 0, 10, true)]
        [TestCase(-1, 0, 10, true)]
        [TestCase(11, 0, 10, true)]
        [TestCase(0, 0, 10, false)]
        [TestCase(10, 0, 10, false)]
        public void Should_fail_when_value_lies_outside_of_range(int value, int from, int to, bool inclusive)
        {
            var config = new TestConfig { Field = value };
            var constraint = new RangeConstraint<TestConfig, int>(c => c.Field, from, to, inclusive);

            constraint.ShouldFailOn(config);
        }

        private class TestConfig
        {
            public int Field;
        }
    }
}