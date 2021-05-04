using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Tests.Integration;
using Vostok.Configuration.Validation;
using Vostok.Configuration.Validation.Constraints;

namespace Vostok.Configuration.Tests.Validation
{
    [TestFixture]
    internal class ConstraintsValidator_Tests : TreeConstructionSet
    {
        [Test]
        public void Should_produce_no_errors_on_valid_configuration_data()
        {
            using (var provider = new ConfigurationProvider())
            {
                provider.SetupSourceFor<TestConfig>(new TestConfigurationSource(Object(Value("Number", "5"), Value("String", "something"))));

                var config = provider.Get<TestConfig>();

                config.Number.Should().Be(5);
                config.String.Should().Be("something");
            }
        }

        [Test]
        public void Should_produce_validation_errors_on_incorrect_configuration_data()
        {
            using (var provider = new ConfigurationProvider())
            {
                provider.SetupSourceFor<TestConfig>(new TestConfigurationSource(Object(Value("Number", "15"), Value("String", "   "))));

                Action getConfig = () => provider.Get<TestConfig>();

                getConfig.Should().Throw<SettingsValidationException>().Which.ShouldBePrinted();
            }
        }

        [ValidateBy(typeof(TestConfigValidator))]
        private class TestConfig
        {
            public int Number;
            public string String;
        }

        private class TestConfigValidator : ConstraintsValidator<TestConfig>
        {
            protected override IEnumerable<Constraint<TestConfig>> GetConstraints()
            {
                yield return new NotNullOrWhitespaceConstraint<TestConfig>(settings => settings.String);
                yield return new RangeConstraint<TestConfig, int>(settings => settings.Number, 2, 10);
            }
        }
    }
}