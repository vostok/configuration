using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class LessOrEqualConstraint_Tests
    {
        private LessOrEqualConstraint<TestConfig, int> constraint;
        private TestConfig config;

        [SetUp]
        public void TestSetup()
        {
            constraint = new LessOrEqualConstraint<TestConfig, int>(c => c.Field1, c => c.Field2);
            config = new TestConfig();
        }

        [Test]
        public void Should_fail_when_left_field_value_is_greater()
        {
            constraint.ShouldFailOn(config);
        }

        [Test]
        public void Should_pass_when_left_field_value_is_equal()
        {
            config.Field2 = config.Field1;

            constraint.ShouldPassOn(config);
        }

        [Test]
        public void Should_pass_when_left_field_value_is_less()
        {
            config.Field1 = config.Field2 - 1;

            constraint.ShouldPassOn(config);
        }

        private class TestConfig
        {
            public int Field1 = 10;
            public int Field2 = 5;
        }
    }
}