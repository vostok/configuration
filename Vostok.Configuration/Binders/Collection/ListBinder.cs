using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Collection
{
    internal class ListBinder<T> :
        ISafeSettingsBinder<List<T>>,
        ISafeSettingsBinder<IList<T>>,
        ISafeSettingsBinder<ICollection<T>>
    {
        private readonly ISafeSettingsBinder<T> elementBinder;

        public ListBinder(ISafeSettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public SettingsBindingResult<List<T>> Bind(ISettingsNode settings)
        {
            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<List<T>>(settings);

            var results = settings.Children.Select((n, i) => (index: i, value: elementBinder.BindOrDefault(n))).ToList();

            var value = results.Select(r => r.value.Value).ToList();
            var errors = results.SelectMany(r => r.value.Errors.ForIndex(r.index));
            return SettingsBindingResult.Create(value, errors);
        }

        SettingsBindingResult<IList<T>> ISafeSettingsBinder<IList<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, IList<T>>();

        SettingsBindingResult<ICollection<T>> ISafeSettingsBinder<ICollection<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, ICollection<T>>();
    }
}