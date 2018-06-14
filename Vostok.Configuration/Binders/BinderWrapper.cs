using System.Linq;
using System.Reflection;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper : ISettingsBinder<object>
    {
        private readonly object binder;

        public BinderWrapper(object binder) =>
            this.binder = binder;

        private MethodInfo BinderBindMethod =>
            binder.GetType().GetMethods().First(m => m.Name == nameof(Bind));

        public object Bind(ISettingsNode rawSettings) =>
            BinderBindMethod.Invoke(binder, new object[] {rawSettings});
    }
}