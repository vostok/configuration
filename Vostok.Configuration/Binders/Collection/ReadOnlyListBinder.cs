using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

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

        public SettingsBindingResult<T[]> Bind(ISettingsNode settings)
        {
            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<T[]>(settings);
            
            var results = settings.Children.Select((n, i) => (index: i, value: elementBinder.BindOrDefault(n))).ToList();

            var value = results.Select(r => r.value.Value).ToArray();
            var errors = results.SelectMany(r => r.value.Errors.ForIndex(r.index));
            return SettingsBindingResult.Create(value, errors);
        }

        SettingsBindingResult<IReadOnlyCollection<T>> ISettingsBinder<IReadOnlyCollection<T>>.Bind(ISettingsNode settings) => 
            Bind(settings).Convert<T[], IReadOnlyCollection<T>>();

        SettingsBindingResult<IReadOnlyList<T>> ISettingsBinder<IReadOnlyList<T>>.Bind(ISettingsNode settings) => 
            Bind(settings).Convert<T[], IReadOnlyList<T>>();

        SettingsBindingResult<IEnumerable<T>> ISettingsBinder<IEnumerable<T>>.Bind(ISettingsNode settings) => 
            Bind(settings).Convert<T[], IEnumerable<T>>();
    }
}