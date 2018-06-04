using System.Linq;
using System.Reflection;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper : ISettingsBinder<object>
    {
        private readonly object binder;
        private readonly BinderAttribute binderAttribute;

        public BinderWrapper(object binder, BinderAttribute binderAttribute)
        {
            this.binder = binder;
            this.binderAttribute = binderAttribute;
        }

        public object Bind(IRawSettings rawSettings)
        {
            var method = GetBinderBindMethod();

            if (binderAttribute == BinderAttribute.IsOptional)
                try
                {
                    return method.Invoke(binder, new object[] { rawSettings });
                }
                catch { return default; }   //todo: maybe need Default method (actual for nullable)

            return method.Invoke(binder, new object[] {rawSettings});
        }

        private MethodInfo GetBinderBindMethod() =>
            binder.GetType().GetMethods().First(m => m.Name == nameof(Bind));
    }
}