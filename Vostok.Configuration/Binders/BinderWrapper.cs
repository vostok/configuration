using System.Linq;
using System.Reflection;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper : ISettingsBinder<object>
    {
        private readonly object binder;

        public BinderWrapper(object binder) =>
            this.binder = binder;

        public object Bind(IRawSettings rawSettings) =>
            GetBinderBindMethod().Invoke(binder, new object[] {rawSettings});

        private MethodInfo GetBinderBindMethod() =>
            binder.GetType().GetMethods().First(m => m.Name == nameof(Bind));
    }
}