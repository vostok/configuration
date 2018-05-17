using System.Linq;

namespace Vostok.Configuration.Binders
{
    internal class ArrayBinder<T> :
        ISettingsBinder<T[]>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public ArrayBinder(ISettingsBinder<T> elementBinder)
        {
            this.elementBinder = elementBinder;
        }

        public T[] Bind(RawSettings settings) =>
            (settings.Children ?? Enumerable.Empty<RawSettings>())
            .Select(n => elementBinder.Bind(n))
            .ToArray();
    }
}