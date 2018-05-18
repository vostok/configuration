using System;
using System.Linq;
using System.Reflection;

namespace Vostok.Configuration.Binders
{
    internal class StructBinder<T> :
        ISettingsBinder<T>
    {
        private readonly ISettingsBinderFactory binderFactory;

        public StructBinder(ISettingsBinderFactory binderFactory)
        {
            this.binderFactory = binderFactory;
        }

        public T Bind(RawSettings settings)
        {
            var type = typeof(T);
            var instance = Activator.CreateInstance(type);

            var defaultAttrOption = BinderAttribute.IsOptional;
            if (type.GetCustomAttributes().Any(a => a.GetType() == typeof(RequiredByDefaultAttribute)))
                defaultAttrOption = BinderAttribute.IsRequired;

            foreach (var field in type.GetFields())
            {
                var binderAttribute = field.GetCustomAttributes().GetAttributes(defaultAttrOption);
                var res = GetValue(field.FieldType, field.Name, binderAttribute, settings);
                field.SetValue(instance, res);
            }
            foreach (var prop in type.GetProperties().Where(p => p.CanWrite))
            {
                var binderAttribute = prop.GetCustomAttributes().GetAttributes(defaultAttrOption);
                var res = GetValue(prop.PropertyType, prop.Name, binderAttribute, settings);
                prop.SetValue(instance, res);
            }

            return (T)instance;
        }

        private object GetValue(Type type, string name, BinderAttribute binderAttribute, RawSettings settings)
        {
            object SetDefault(Type t) =>
                t.IsClass || t.IsNullable() ? null : Activator.CreateInstance(t);
            object DefautByOptionalOrThrow(BinderAttribute attr, Type t, string msg) =>
                attr == BinderAttribute.IsOptional ? SetDefault(t) : throw new InvalidCastException(msg);

            var binder = binderFactory.CreateForType(type, binderAttribute);
            if (settings.ChildrenByKey == null || !settings.ChildrenByKey.ContainsKey(name))
                return DefautByOptionalOrThrow(binderAttribute, type, $"Required key \"{name}\" is absent");
            else
            {
                var rs = settings.ChildrenByKey[name];
                if ((type.IsNullable() || type.IsClass) && rs.IsEmpty())
                    return DefautByOptionalOrThrow(binderAttribute, type, $"Not nullable required value of field/property \"{name}\" is null");
                else
                    return binder.Bind(rs);
            }
        }
    }
}