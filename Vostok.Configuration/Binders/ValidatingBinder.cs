using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class ValidatingBinder : ISettingsBinder
    {
        private readonly ISettingsBinder binder;

        public ValidatingBinder(ISettingsBinder binder)
        {
            this.binder = binder;
        }

        public TSettings Bind<TSettings>(ISettingsNode rawSettings)
        {
            var value = binder.Bind<TSettings>(rawSettings);
            Validate(value);
            return value;
        }

        private static void Validate<TSettings>(TSettings value)
        {
            var type = typeof(TSettings);

            var errors = Validate(type, value, "").ToList();
            if (errors.Any())
            {
                throw new SettingsValidationException(string.Join(Environment.NewLine, errors));
            }
        }

        private static IEnumerable<string> Validate(Type type, object value, string prefix)
        {
            if (value == null)
                yield break;
            if (!(type.GetCustomAttributes(typeof(ValidateByAttribute), false).FirstOrDefault() is ValidateByAttribute validAttribute))
                yield break;

            var validator = Activator.CreateInstance(validAttribute.ValidatorType);
            var validateMethod = validator.GetType().GetMethod(nameof(ISettingsValidator<object>.Validate));
            if (validateMethod == null)
                yield break; // TODO(krait): Report error.
            foreach (var error in (IEnumerable<string>)validateMethod.Invoke(validator, new[] {value}))
                yield return prefix + error;

            foreach (var field in type.GetFields())
            {
                foreach (var error in Validate(field.FieldType, field.GetValue(value), field.Name))
                    yield return prefix + error;
            }

            foreach (var prop in type.GetProperties())
            {
                foreach (var error in Validate(prop.PropertyType, prop.GetValue(value), prop.Name))
                    yield return prefix + error;
            }
        }
    }
}