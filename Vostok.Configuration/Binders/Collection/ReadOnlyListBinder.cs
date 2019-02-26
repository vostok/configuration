using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ReadOnlyListBinder<T> :
        ISafeSettingsBinder<T[]>,
        ISafeSettingsBinder<IReadOnlyCollection<T>>,
        ISafeSettingsBinder<IReadOnlyList<T>>,
        ISafeSettingsBinder<IEnumerable<T>>
    {
        private readonly ISafeSettingsBinder<T> elementBinder;

        public ReadOnlyListBinder(ISafeSettingsBinder<T> elementBinder) =>
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

        SettingsBindingResult<IReadOnlyCollection<T>> ISafeSettingsBinder<IReadOnlyCollection<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IReadOnlyCollection<T>>();

        SettingsBindingResult<IReadOnlyList<T>> ISafeSettingsBinder<IReadOnlyList<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IReadOnlyList<T>>();

        SettingsBindingResult<IEnumerable<T>> ISafeSettingsBinder<IEnumerable<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<T[], IEnumerable<T>>();
    }
}