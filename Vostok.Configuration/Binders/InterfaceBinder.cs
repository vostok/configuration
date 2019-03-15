using System;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class InterfaceBinder<TInterface> : ISafeSettingsBinder<TInterface>, INullValuePolicy
    {
        private readonly object classBinder;
        private readonly Func<ISettingsNode, SettingsBindingResult<TInterface>> callBindMethod;

        public InterfaceBinder(ISettingsBinderProvider binderProvider)
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(TInterface));
            var classBinderType = typeof(ClassStructBinder<>).MakeGenericType(implType);

            classBinder = Activator.CreateInstance(classBinderType, binderProvider);

            var bindMethod = classBinderType.GetMethod(nameof(ISettingsBinder.Bind));
            if (bindMethod == null)
                throw new NullReferenceException($"Can't find '{nameof(ISettingsBinder.Bind)}' method on '{classBinderType.FullName}' type.");

            var bindingResultWrapperType = typeof(SettingsBindingResultWrapper<,>).MakeGenericType(typeof(TInterface), implType);

            callBindMethod = node =>
            {
                var bindingResult = bindMethod.Invoke(classBinder, new object[] {node});
                var bindingResultWrapper = Activator.CreateInstance(bindingResultWrapperType, bindingResult);
                return (SettingsBindingResult<TInterface>)bindingResultWrapper;
            };
        }

        public SettingsBindingResult<TInterface> Bind(ISettingsNode rawSettings) => callBindMethod(rawSettings);

        public bool IsNullValue(ISettingsNode node) => ((INullValuePolicy)classBinder).IsNullValue(node);
    }
}