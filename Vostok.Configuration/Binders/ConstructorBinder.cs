using System;
using System.Linq;
using System.Reflection;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class ConstructorBinder<T> : ISafeSettingsBinder<T>, INullValuePolicy
    {
        private readonly ISettingsBinderProvider binderProvider;

        public ConstructorBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider;

        public static bool CanBeUsedFor(Type type) => FindConstructor(type) != null;

        public SettingsBindingResult<T> Bind(ISettingsNode settings) =>
            SettingsBindingResult.Catch(() => BindInternal(settings));

        public bool IsNullValue(ISettingsNode node)
        {
            if (node.IsNullValue())
                return true;

            if (typeof(T).IsValueType)
                return false;

            return node is ValueNode valueNode && valueNode.Value?.ToLower() == "null";
        }

        private static ConstructorInfo FindConstructor(Type type)
        {
            var constructors = ConstructorsHelper.GetConstructors(type);
            if (constructors.Count == 1 && constructors[0].GetParameters().Length == 1)
                return constructors[0];
            return null;
        }

        private SettingsBindingResult<T> BindInternal(ISettingsNode settings)
        {
            var type = typeof(T);

            var constructor = FindConstructor(type);
            if (constructor == null)
                return SettingsBindingResult.Error<T>("Failed to find appropriate constructor.");

            var parameter = constructor.GetParameters()[0];

            var binder = binderProvider.CreateFor(parameter.ParameterType);
            if (binder == null)
                return SettingsBindingResult.BinderNotFound<T>(type);

            var argument = binder.Bind(settings);
            if (argument.Errors.Any())
                return SettingsBindingResult.Errors<T>(argument.Errors);

            var instance = (T)Activator.CreateInstance(type, argument.Value);
            return SettingsBindingResult.Success(instance);
        }
    }
}