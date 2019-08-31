using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

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

            var errors = Validate(type, value, new HashSet<Type>()).ToList();
            if (errors.Any())
                throw new SettingsValidationException(
                    $"Validation of settings of type '{typeof(TSettings)}' failed:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, errors.Select(e => "\t- " + e)));
        }

        private static IEnumerable<string> Validate(Type type, object value, HashSet<Type> visitedTypes)
        {
            if (!visitedTypes.Add(type))
                yield break;

            var attribute = AttributeHelper.Get<ValidateByAttribute>(type);
            if (attribute != null)
            {
                var validator = Activator.CreateInstance(attribute.ValidatorType);
                var validateMethod = validator.GetType().GetMethod(nameof(ISettingsValidator<object>.Validate), new[] {type});
                if (validateMethod == null)
                    throw new SettingsValidationException($"Type '{validator.GetType()}' specified as validator for settings of type '{type}' does not contain a suitable {nameof(ISettingsValidator<object>.Validate)} method.");

                foreach (var error in (IEnumerable<string>) validateMethod.Invoke(validator, new[] {value}))
                    yield return error;
            }

            if (value == null)
                yield break;

            foreach (var field in type.GetInstanceFields())
            {
                foreach (var error in Validate(field.FieldType, field.GetValue(value), visitedTypes))
                    yield return FormatError(field.Name, error);
            }

            var properties = type.GetInstanceProperties();

            if (type.IsInterface)
                properties = properties.Concat(type.GetInterfaces().SelectMany(iface => iface.GetInstanceProperties()));

            foreach (var prop in properties)
            {
                object propertyValue;

                try
                {
                    propertyValue = prop.GetValue(value);
                }
                catch
                {
                    continue;
                }

                foreach (var error in Validate(prop.PropertyType, propertyValue, visitedTypes))
                    yield return FormatError(prop.Name, error);
            }

            visitedTypes.Remove(type);
        }

        private static string FormatError(string prefix, string error) => prefix + ": " + error;
    }
}