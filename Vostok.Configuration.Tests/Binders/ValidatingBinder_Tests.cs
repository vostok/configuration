using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    public class ValidatingBinder_Tests
    {
        [SetUp]
        public void TestSetup()
        {
        }

        [Test]
        public void Should_build_correct_exception_message()
        {
            new Action(() => Validate(new Settings()))
                .Should()
                .Throw<SettingsValidationException>()
                .WithMessage(
                    $@"Validation of settings of type '{typeof(Settings)}' failed:
	- Value must not be null!
	- Inner: Value must not be null!
	- Inner: Inner: Value must not be null!")
                .Which.ShouldBePrinted();
        }

        [Test]
        public void Should_pass_validation_if_there_is_no_ValidateBy_attribute()
        {
            Validate(new SettingsWithoutValidation());
        }

        [Test]
        public void Should_pass_validation_if_there_is_no_errors()
        {
            Validate(new Settings {Value = "x", Inner = new Settings1 {Value = "y", Inner = new Settings2 {Value = "z"}}});
        }

        [Test]
        public void Should_throw_if_validator_class_does_not_have_suitable_interface()
        {
            new Action(() => Validate(new SettingsWithBadValidator()))
                .Should()
                .Throw<SettingsValidationException>()
                .Which.ShouldBePrinted();
        }

        [Test]
        public void Should_validate_null_values()
        {
            new Action(() => Validate<Settings>(null))
                .Should()
                .Throw<SettingsValidationException>()
                .Which.ShouldBePrinted();
        }

        [Test]
        public void Should_validate_nested_types_even_if_enclosing_type_has_no_validator()
        {
            new Action(
                    () => Validate(
                        new NonValidatedSettings
                        {
                            Settings = new Settings
                            {
                                Value = "value",
                                Inner = new Settings1
                                {
                                    Value = "value2",
                                    Inner = new Settings2
                                    {
                                        Value = null
                                    }
                                }
                            }
                        }))
                .Should()
                .Throw<SettingsValidationException>()
                .Which.ShouldBePrinted();
        }

        [Test]
        public void Should_validate_properties_of_base_interfaces()
        {
            new Action(() => Validate(Substitute.For<IDerived>()))
                .Should()
                .Throw<SettingsValidationException>()
                .Which.ShouldBePrinted();
        }

        [Test]
        public void Should_not_fail_when_encountering_an_exception_arising_from_property_getter()
        {
            Validate(new Settings3());
        }

        private static void Validate<TSettings>(TSettings settings)
        {
            var binder = Substitute.For<ISettingsBinder>();
            binder.Bind<TSettings>(null).Returns(settings);
            new ValidatingBinder(binder).Bind<TSettings>(null);
        }

        [ValidateBy(typeof(object))]
        public class SettingsWithBadValidator
        {
        }

        public class SettingsWithoutValidation
        {
        }

        public class NonValidatedSettings
        {
            public Settings Settings { get; set; }
        }

        [ValidateBy(typeof(Validator))]
        public class Settings
        {
            public string Value { get; set; }

            public Settings1 Inner { get; set; } = new Settings1();
        }

        [ValidateBy(typeof(Validator))]
        public class Settings1
        {
            public string Value { get; set; }

            public Settings2 Inner { get; set; } = new Settings2();
        }

        [ValidateBy(typeof(Validator))]
        public class Settings2
        {
            public string Value { get; set; }
        }

        public class Settings3
        {
            public string Value
            {
                get => throw new Exception("Failed, sorry.");
            }
        }

        public interface IBase
        {
            Settings Settings { get; }
        }

        public interface IDerived : IBase
        {
        }

        public class Validator : ISettingsValidator<Settings>, ISettingsValidator<Settings1>, ISettingsValidator<Settings2>
        {
            public IEnumerable<string> Validate(Settings settings)
            {
                if (settings == null)
                {
                    yield return "Settings must not be null!";
                    yield break;
                }

                if (settings.Value == null)
                    yield return $"{nameof(settings.Value)} must not be null!";
            }

            public IEnumerable<string> Validate(Settings1 settings)
            {
                if (settings.Value == null)
                    yield return $"{nameof(settings.Value)} must not be null!";
            }

            public IEnumerable<string> Validate(Settings2 settings)
            {
                if (settings.Value == null)
                    yield return $"{nameof(settings.Value)} must not be null!";
            }
        }
    }
}
