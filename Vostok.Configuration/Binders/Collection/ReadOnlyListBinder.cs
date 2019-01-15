using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ReadOnlyListBinder<T> :
        ISettingsBinder<T[]>,
        ISettingsBinder<IReadOnlyCollection<T>>,
        ISettingsBinder<IReadOnlyList<T>>,
        ISettingsBinder<IEnumerable<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public ReadOnlyListBinder(ISettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public T[] Bind(ISettingsNode settings) => settings?.Children.Select(n => elementBinder.Bind(n)).ToArray();

        IReadOnlyCollection<T> ISettingsBinder<IReadOnlyCollection<T>>.Bind(ISettingsNode settings) => Bind(settings);

        IReadOnlyList<T> ISettingsBinder<IReadOnlyList<T>>.Bind(ISettingsNode settings) => Bind(settings);

        IEnumerable<T> ISettingsBinder<IEnumerable<T>>.Bind(ISettingsNode settings) => Bind(settings);
    }
}