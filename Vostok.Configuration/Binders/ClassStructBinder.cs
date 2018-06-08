using System;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.SettingsTree;

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
            {
                var binderAttribute = field.GetCustomAttributes().GetBinderAttribute(defaultAttrOption);
                var res = GetValue(field.FieldType, field.Name, binderAttribute, settings, field.GetValue(instance) ?? field.FieldType.Default());
                field.SetValue(instance, res);
            }

            foreach (var prop in type.GetProperties().Where(p => p.CanWrite))
            {
                var binderAttribute = prop.GetCustomAttributes().GetBinderAttribute(defaultAttrOption);
                var res = GetValue(prop.PropertyType, prop.Name, binderAttribute, settings, prop.GetValue(instance) ?? prop.PropertyType.Default());
                prop.SetValue(instance, res);
            }

            return (T)instance;
        }

        private object GetValue(Type type, string name, BinderAttribute binderAttribute, ISettingsNode settings, object defaultValue)
        {
            object GetDefaultIfOptionalOrThrow(BinderAttribute attr, Type t, string msg) =>
                attr == BinderAttribute.IsOptional ? t.Default() : throw new InvalidCastException(msg);

            SettingsNode.CheckSettings(settings, false);

            if (settings[name] == null)
                return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: required key \"{name}\" is absent");
            else
            {
                var rs = settings[name];
                if ((type.IsNullable() || !type.IsValueType) && rs.Value == null && !rs.Children.Any())
                    return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: not nullable required value of field/property \"{name}\" is null");
                else
                    try
                    {
                        return binderFactory.CreateForType(type).Bind(rs);
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