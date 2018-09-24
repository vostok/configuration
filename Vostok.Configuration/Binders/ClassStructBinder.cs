using System;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class ClassAndStructBinder<T> : ISettingsBinder<T>
    {
        private readonly ISettingsBinderFactory binderFactory;

        public ClassAndStructBinder(ISettingsBinderFactory binderFactory) =>
            this.binderFactory = binderFactory;

        public T Bind(ISettingsNode settings)
        {
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var defaultAttrOption = BinderAttribute.IsOptional;
            if (type.GetCustomAttributes().Any(a => a.GetType() == typeof(RequiredByDefaultAttribute)))
                defaultAttrOption = BinderAttribute.IsRequired;

            foreach (var field in type.GetFields())
                field.SetValue(
                    instance,
                    GetValue(field.FieldType, field.Name, field.GetCustomAttributes().GetBinderAttribute(defaultAttrOption), settings, field.GetValue(instance)));

            foreach (var prop in type.GetProperties().Where(p => p.CanWrite))
                prop.SetValue(
                    instance,
                    GetValue(prop.PropertyType, prop.Name, prop.GetCustomAttributes().GetBinderAttribute(defaultAttrOption), settings, prop.GetValue(instance)));

            return (T) instance;
        }

        private object GetValue(Type type, string name, BinderAttribute binderAttribute, ISettingsNode settings, object defaultValue)
        {
            defaultValue = defaultValue ?? type.Default();

            object GetDefaultIfOptionalOrThrow(BinderAttribute attr, Type t, string msg) =>
                attr == BinderAttribute.IsOptional ? t.Default() : throw new InvalidCastException(msg);

            SettingsNode.CheckSettings(settings, false, $"{name} of \"{type.Name}\"");

            var rs = settings[name];
            if (rs == null)
                return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: required key \"{name}\" is absent");
            else
            {
                if ((type.IsNullable() || !type.IsValueType) && rs.Value == null && !rs.Children.Any())
                    return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: not nullable required value of field/property \"{name}\" is null");
                else
                    try
                    {
                        return binderFactory.CreateFor(type).Bind(rs);
                    }
                    catch
                    {
                        if (binderAttribute == BinderAttribute.IsOptional)
                            return defaultValue;
                        throw;
                    }
            }
        }
    }
}