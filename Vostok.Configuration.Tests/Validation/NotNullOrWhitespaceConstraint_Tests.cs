using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class NotNullOrWhitespaceConstraint_Tests
    {
        private NotNullOrWhitespaceConstraint<TestConfig> constraint;
        private TestConfig config;

        [SetUp]
        public void TestSetup()
        {
            constraint = new NotNullOrWhitespaceConstraint<TestConfig>(c => c.Field);
            config = new TestConfig();
        }

        [Test]
        public void Should_pass_when_field_is_not_null_or_empty()
        {
            constraint.ShouldPassOn(config);
        }

        [Test]
        public void Should_fail_when_field_is_whitespace()
        {
            config.Field = "  ";

            constraint.ShouldFailOn(config);
        }

        [Test]
        public void Should_fail_when_field_is_null()
        {
            config.Field = null;

            constraint.ShouldFailOn(config);
        }

        [Test]
        public void Should_fail_when_field_is_empty()
        {
            config.Field = string.Empty;

            constraint.ShouldFailOn(config);
        }

        private class TestConfig
        {
            public string Field = "123";
        }
    }
}