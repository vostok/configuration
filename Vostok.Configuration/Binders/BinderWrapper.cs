using System.Linq;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class BinderWrapper: ISettingsBinder<object>
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
            var method = binder.GetType().GetMethods().First(m => m.Name == nameof(Bind));

            if (binderAttribute == BinderAttribute.IsOptional)
                try
                {
                    return method.Invoke(binder, new object[] { rawSettings });
                }
                catch { return default; }   //todo: maybe need Default method

            return method.Invoke(binder, new object[] {rawSettings});
        }
    }
}