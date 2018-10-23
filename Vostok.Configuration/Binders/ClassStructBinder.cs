using System;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class ClassStructBinder<T> : ISettingsBinder<T>
    {
        private readonly ISettingsBinderProvider binderProvider;

        public ClassStructBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider;

        public T Bind(ISettingsNode settings)
        {
            if (settings.IsNull() && !typeof(T).IsValueType) // TODO(krait): Test this behavior.
                return default;

            if (!(settings is ObjectNode))
                throw new BindingException($"A settings node of type '{settings.GetType()}' cannot be bound by {nameof(ClassStructBinder<T>)}.");

            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var requiredByDefault = type.GetCustomAttribute<RequiredByDefaultAttribute>() != null;

            foreach (var field in type.GetFields())
                field.SetValue(
                    instance,
                    GetValue(field.FieldType, field.Name, IsRequired(field, requiredByDefault), settings, field.GetValue(instance)));

            foreach (var property in type.GetProperties().Where(p => p.CanWrite))
                property.SetValue(
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
            var value = settings[name];
            if (value == null)
                return isRequired ? throw new BindingException($"Required field or property '{name}' must have a non-null value.") : defaultValue;

            return binderProvider.CreateFor(type).Bind(value);
        }
    }
}