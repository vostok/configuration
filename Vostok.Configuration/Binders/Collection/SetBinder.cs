using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Collection
{
    internal class SetBinder<T> :
        ISettingsBinder<HashSet<T>>,
        ISettingsBinder<ISet<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public SetBinder(ISettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public HashSet<T> Bind(ISettingsNode settings) => settings != null ? new HashSet<T>(settings.Children.Select(n => elementBinder.Bind(n))) : null;

        ISet<T> ISettingsBinder<ISet<T>>.Bind(ISettingsNode settings) => Bind(settings);
    }
}