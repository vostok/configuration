using System;
using NUnit.Framework;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class UniqueConstraint_Tests
    {
        private UniqueConstraint<TestConfig, string> constraint;
        private TestConfig config;

        [SetUp]
        public void TestSetup()
        {
            constraint = new UniqueConstraint<TestConfig, string>(StringComparer.OrdinalIgnoreCase, c => c.Field1, c => c.Field2, c => c.Field3);
            config = new TestConfig();
        }

        [Test]
        public void Should_pass_when_all_selected_fields_have_unique_values()
        {
            constraint.ShouldPassOn(config);
        }

        [Test]
        public void Should_fail_when_some_of_selected_fields_have_equal_values()
        {
            config.Field3 = "A";

            constraint.ShouldFailOn(config);
        }

        private class TestConfig
        {
            public readonly string Field1 = "a";
            public readonly string Field2 = "b";
            public string Field3 = "c";
        }
    }
}