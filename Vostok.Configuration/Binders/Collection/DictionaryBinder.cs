using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Collection
{
    internal class DictionaryBinder<T1, T2> :
        ISafeSettingsBinder<Dictionary<T1, T2>>,
        ISafeSettingsBinder<IDictionary<T1, T2>>,
        ISafeSettingsBinder<IReadOnlyDictionary<T1, T2>>
    {
        private readonly ISafeSettingsBinder<T1> keyBinder;
        private readonly ISafeSettingsBinder<T2> valueBinder;

        public DictionaryBinder(ISafeSettingsBinder<T1> keyBinder, ISafeSettingsBinder<T2> valueBinder)
        {
            this.keyBinder = keyBinder;
            this.valueBinder = valueBinder;
        }

        public SettingsBindingResult<Dictionary<T1, T2>> Bind(ISettingsNode settings)
        {
            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<Dictionary<T1, T2>>(settings);

            var results = settings.Children.Select(
                    n =>
                        (index: n.Name, key: keyBinder.BindOrDefault(new ValueNode(n.Name)), value: valueBinder.BindOrDefault(n)))
                .ToList();

            var errors = results.SelectMany(
                    p => p.key.Errors.ForIndex(p.index).Concat(p.value.Errors.ForIndex(p.index)))
                .ToList();
            
            if (errors.Any())
                return SettingsBindingResult.Errors<Dictionary<T1, T2>>(errors);
            
            return SettingsBindingResult.Success(results.ToDictionary(p => p.key.Value, p => p.value.Value));
        }

        SettingsBindingResult<IDictionary<T1, T2>> ISafeSettingsBinder<IDictionary<T1, T2>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<Dictionary<T1, T2>, IDictionary<T1, T2>>();

        SettingsBindingResult<IReadOnlyDictionary<T1, T2>> ISafeSettingsBinder<IReadOnlyDictionary<T1, T2>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<Dictionary<T1, T2>, IReadOnlyDictionary<T1, T2>>();
    }
}