using System.Reflection;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper : ISettingsBinder<object>
    {
        private readonly object binder;

        public BinderWrapper(object binder) =>
            this.binder = binder;

        private MethodInfo BinderBindMethod =>
            binder.GetType().GetMethod(nameof(Bind));

        public object Bind(ISettingsNode rawSettings) =>
            BinderBindMethod.Invoke(binder, new object[] {rawSettings});
    }
}