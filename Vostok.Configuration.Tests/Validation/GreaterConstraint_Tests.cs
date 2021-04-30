using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class GreaterConstraint_Tests
    {
        [SetUp]
        public void TestSetup()
        {
            constraint = new GreaterConstraint<TestConfig, int>(c => c.Field1, c => c.Field2);
            config = new TestConfig();
        }

        [Test]
        public void Should_pass_when_left_field_value_is_greater()
        {
            constraint.ShouldPassOn(config);
        }

        [Test]
        public void Should_fail_when_left_field_value_is_equal()
        {
            config.Field2 = config.Field1;

            constraint.ShouldFailOn(config);
        }

        [Test]
        public void Should_fail_when_left_field_value_is_less()
        {
            config.Field1 = config.Field2 - 1;

            constraint.ShouldFailOn(config);
        }

        private GreaterConstraint<TestConfig, int> constraint;
        private TestConfig config;

        private class TestConfig
        {
            public int Field1 = 10;
            public int Field2 = 5;
        }
    }
}