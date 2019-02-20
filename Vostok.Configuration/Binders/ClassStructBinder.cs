using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;
using OptionalAttribute = Vostok.Configuration.Abstractions.Attributes.OptionalAttribute;

namespace Vostok.Configuration.Binders
{
    internal class ClassStructBinder<T> : ISettingsBinder<T>
    {
        private readonly ISettingsBinderProvider binderProvider;

        public ClassStructBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider;

        public T Bind(ISettingsNode settings)
        {
            if (settings != null && !(settings is ObjectNode))
                throw new SettingsBindingException($"A settings node of type '{settings.GetType()}' cannot be bound by {nameof(ClassStructBinder<T>)}.");
            
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var requiredByDefault = type.GetCustomAttribute<RequiredByDefaultAttribute>() != null;

            foreach (var field in type.GetInstanceFields())
                field.SetValue(
                    instance,
                    GetValue(field.FieldType, field.Name, IsRequired(field, requiredByDefault), settings, field.GetValue(instance)));

            foreach (var property in type.GetInstanceProperties())
                property.ForceSetValue(
                    instance,
                    GetValue(property.PropertyType, property.Name, IsRequired(property, requiredByDefault), settings, property.GetValue(instance)));

            return (T)instance;
        }

        private static bool IsRequired(MemberInfo member, bool requiredByDefault)
        {
            if (requiredByDefault)
                return member.GetCustomAttribute<OptionalAttribute>() == null;

            return member.GetCustomAttribute<RequiredAttribute>() != null;
        }

        private object GetValue(Type type, string name, bool isRequired, ISettingsNode settings, object defaultValue)
        {
            var binder = binderProvider.CreateFor(type);

            var value = settings?[name];
            if (value != null && !value.IsNullValue(binder))
                return binder.Bind(value);
            
            if (isRequired)
                throw new SettingsBindingException($"Required field or property '{name}' must have a non-null value.");

            return value == null ? defaultValue : null;

        }
    }
}