using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders
{
    internal class ListBinder<T> :
        ISettingsBinder<List<T>>,
        ISettingsBinder<IList<T>>,
        ISettingsBinder<IEnumerable<T>>,
        ISettingsBinder<ICollection<T>>,
        ISettingsBinder<IReadOnlyCollection<T>>,
        ISettingsBinder<IReadOnlyList<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public ListBinder(ISettingsBinder<T> elementBinder)
        {
            this.elementBinder = elementBinder;
        }

        public List<T> Bind(IRawSettings settings)
        {
            RawSettings.CheckSettings(settings);

            return settings.Children
                .Select(n => elementBinder.Bind(n))
                .ToList();
        }

        IList<T> ISettingsBinder<IList<T>>.Bind(IRawSettings settings) => Bind(settings);
        IEnumerable<T> ISettingsBinder<IEnumerable<T>>.Bind(IRawSettings settings) => Bind(settings);
        ICollection<T> ISettingsBinder<ICollection<T>>.Bind(IRawSettings settings) => Bind(settings);
        IReadOnlyCollection<T> ISettingsBinder<IReadOnlyCollection<T>>.Bind(IRawSettings settings) => Bind(settings);
        IReadOnlyList<T> ISettingsBinder<IReadOnlyList<T>>.Bind(IRawSettings settings) => Bind(settings);
    }
}