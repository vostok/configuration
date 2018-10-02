using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ListBinder<T> :
        ISettingsBinder<List<T>>,
        ISettingsBinder<IList<T>>,
        ISettingsBinder<ICollection<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public ListBinder(ISettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public List<T> Bind(ISettingsNode settings) => settings.Children.Select(n => elementBinder.Bind(n)).ToList();

        IList<T> ISettingsBinder<IList<T>>.Bind(ISettingsNode settings) => Bind(settings);

        ICollection<T> ISettingsBinder<ICollection<T>>.Bind(ISettingsNode settings) => Bind(settings);
    }
}