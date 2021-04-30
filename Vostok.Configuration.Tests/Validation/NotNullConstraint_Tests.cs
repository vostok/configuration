using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class NotNullConstraint_Tests
    {
        [SetUp]
        public void TestSetup()
        {
            constraint = new NotNullConstraint<TestConfig>(c => c.Field);
            config = new TestConfig();
        }

        [Test]
        public void Should_pass_when_field_is_not_null()
        {
            constraint.ShouldPassOn(config);
        }

        [Test]
        public void Should_fail_when_field_is_null()
        {
            config.Field = null;

            constraint.ShouldFailOn(config);
        }

        private NotNullConstraint<TestConfig> constraint;
        private TestConfig config;

        private class TestConfig
        {
            public object Field = new object();
        }
    }
}