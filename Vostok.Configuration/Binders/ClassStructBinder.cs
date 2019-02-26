using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;
using OptionalAttribute = Vostok.Configuration.Abstractions.Attributes.OptionalAttribute;

namespace Vostok.Configuration.Binders
{
    internal class ClassStructBinder<T> : ISafeSettingsBinder<T>
    {
        private readonly ISettingsBinderProvider binderProvider;

        public ClassStructBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider;

        public SettingsBindingResult<T> Bind(ISettingsNode settings)
        {
            if (!(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<T>(settings);

            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var requiredByDefault = type.GetCustomAttribute<RequiredByDefaultAttribute>() != null;

            var errors = new List<SettingsBindingError>();

            foreach (var field in type.GetInstanceFields())
            {
                var result = GetValue(field.FieldType, field.Name, IsRequired(field, requiredByDefault), settings, field.GetValue(instance));
                errors.AddRange(result.Errors.ForProperty(field.Name));
                field.SetValue(instance, result.Value);
            }

            foreach (var property in type.GetInstanceProperties())
            {
                var result = GetValue(property.PropertyType, property.Name, IsRequired(property, requiredByDefault), settings, property.GetValue(instance));
                errors.AddRange(result.Errors.ForProperty(property.Name));
                property.ForceSetValue(instance, result.Value);
            }
            
            if (errors.Any())
                return SettingsBindingResult.Errors<T>(errors);

            return SettingsBindingResult.Success((T)instance);
        }

        private static bool IsRequired(MemberInfo member, bool requiredByDefault)
        {
            if (requiredByDefault)
                return member.GetCustomAttribute<OptionalAttribute>() == null;

            return member.GetCustomAttribute<RequiredAttribute>() != null;
        }

        private SettingsBindingResult<object> GetValue(Type type, string name, bool isRequired, ISettingsNode settings, object defaultValue)
        {
            var binder = binderProvider.CreateFor(type);

            var value = settings?[name];
            if (value != null && !value.IsNullValue(binder))
                return binder.BindOrDefault(value);

            if (isRequired)
                return SettingsBindingResult.RequiredPropertyIsNull<object>(name);

            return SettingsBindingResult.Success(value.IsMissing() ? defaultValue : null);
        }
    }
}