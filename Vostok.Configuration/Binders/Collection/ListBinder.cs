using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ListBinder<T> : CollectionBinder<List<T>, T>,
        ISafeSettingsBinder<List<T>>,
        ISafeSettingsBinder<IList<T>>,
        ISafeSettingsBinder<ICollection<T>>
    {
        public ListBinder(ISafeSettingsBinder<T> elementBinder)
            : base(elementBinder)
        {
        }

        protected override List<T> CreateCollection(IEnumerable<T> elements) =>
            elements.ToList();

        SettingsBindingResult<IList<T>> ISafeSettingsBinder<IList<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, IList<T>>();

        SettingsBindingResult<ICollection<T>> ISafeSettingsBinder<ICollection<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, ICollection<T>>();
    }
}