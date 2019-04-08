using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ReadOnlyListBinder<T> : CollectionBinder<T[], T>,
        ISafeSettingsBinder<T[]>,
        ISafeSettingsBinder<IReadOnlyCollection<T>>,
        ISafeSettingsBinder<IReadOnlyList<T>>,
        ISafeSettingsBinder<IEnumerable<T>>
    {
        public ReadOnlyListBinder(ISafeSettingsBinder<T> elementBinder)
            : base(elementBinder)
        {
        }

        protected override T[] CreateCollection(IEnumerable<T> elements) =>
            elements.ToArray();

        SettingsBindingResult<IReadOnlyCollection<T>> ISafeSettingsBinder<IReadOnlyCollection<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IReadOnlyCollection<T>>();

        SettingsBindingResult<IReadOnlyList<T>> ISafeSettingsBinder<IReadOnlyList<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IReadOnlyList<T>>();

        SettingsBindingResult<IEnumerable<T>> ISafeSettingsBinder<IEnumerable<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IEnumerable<T>>();
    }
}