using System;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;

namespace Vostok.Configuration.Binders.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static bool TryObtainBindByBinder<T>(this MemberInfo member, bool wrap, out ISafeSettingsBinder<T> settingsBinder)
        {
            settingsBinder = null;

            var attribute = member.GetCustomAttribute<BindByAttribute>();
            if (attribute == null)
                return false;

            var memberType = GetMemberType(member);

            var desiredBinderType = typeof(ISettingsBinder<>).MakeGenericType(memberType);
            if (!desiredBinderType.IsAssignableFrom(attribute.BinderType))
                throw new InvalidOperationException($"The type specified in {nameof(BindByAttribute)} must implement {desiredBinderType.ToFriendlyName()}.");

            var binder = Activator.CreateInstance(attribute.BinderType);
            binder = Activator.CreateInstance(typeof(SafeBinderWrapper<>).MakeGenericType(memberType), binder);

            if (wrap)
                binder = Activator.CreateInstance(typeof(BinderWrapper<>).MakeGenericType(memberType), binder);

            settingsBinder = (ISafeSettingsBinder<T>)binder;
            return true;
        }

        private static Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.FieldType;
                case PropertyInfo property:
                    return property.PropertyType;
                case Type type:
                    return type;
                default:
                    throw new NotSupportedException($"Member type '{member.MemberType}' is not supported here.");
            }
        }
    }
}