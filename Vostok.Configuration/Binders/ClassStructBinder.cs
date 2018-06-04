using System;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class ClassAndStructBinder<T> : ISettingsBinder<T>
    {
        private readonly ISettingsBinderFactory binderFactory;

        public ClassAndStructBinder(ISettingsBinderFactory binderFactory) =>
            this.binderFactory = binderFactory;

        public T Bind(IRawSettings settings)
        {
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var defaultAttrOption = BinderAttribute.IsOptional;
            if (type.GetCustomAttributes().Any(a => a.GetType() == typeof(RequiredByDefaultAttribute)))
                defaultAttrOption = BinderAttribute.IsRequired;

            foreach (var field in type.GetFields())
            {
                var binderAttribute = field.GetCustomAttributes().GetBinderAttribute(defaultAttrOption);
                var res = GetValue(field.FieldType, field.Name, binderAttribute, settings);
                field.SetValue(instance, res);
            }
            foreach (var prop in type.GetProperties().Where(p => p.CanWrite))
            {
                var binderAttribute = prop.GetCustomAttributes().GetBinderAttribute(defaultAttrOption);
                var res = GetValue(prop.PropertyType, prop.Name, binderAttribute, settings);
                prop.SetValue(instance, res);
            }

            return (T)instance;
        }

        private object GetValue(Type type, string name, BinderAttribute binderAttribute, IRawSettings settings)
        {
            object GetDefault(Type t) =>
                t.IsClass || t.IsNullable() ? null : Activator.CreateInstance(t);
            object GetDefaultIfOptionalOrThrow(BinderAttribute attr, Type t, string msg) =>
                attr == BinderAttribute.IsOptional ? GetDefault(t) : throw new InvalidCastException(msg);

            RawSettings.CheckSettings(settings, false);

            var binder = binderFactory.CreateForType(type, binderAttribute);
            if (settings[name] == null)
                return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: required key \"{name}\" is absent");
            else
            {
                var rs = (RawSettings)settings[name];
                if ((type.IsNullable() || type.IsClass) && rs.Value == null && !rs.Children.Any())
                    return GetDefaultIfOptionalOrThrow(binderAttribute, type, $"{nameof(ClassAndStructBinder<T>)}: not nullable required value of field/property \"{name}\" is null");
                else
                    return binder.Bind(rs);
            }
        }
    }
}