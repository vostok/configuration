using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Collection
{
    internal class DictionaryBinder<TKey, TValue> :
        ISafeSettingsBinder<Dictionary<TKey, TValue>>,
        ISafeSettingsBinder<IDictionary<TKey, TValue>>,
        ISafeSettingsBinder<IReadOnlyDictionary<TKey, TValue>>
    {
        private readonly ISafeSettingsBinder<TKey> keyBinder;
        private readonly ISafeSettingsBinder<TValue> valueBinder;

        public DictionaryBinder(ISafeSettingsBinder<TKey> keyBinder, ISafeSettingsBinder<TValue> valueBinder)
        {
            this.keyBinder = keyBinder;
            this.valueBinder = valueBinder;
        }

        public SettingsBindingResult<Dictionary<TKey, TValue>> Bind(ISettingsNode settings)
        {
            if (settings.IsNullOrMissing())
                return SettingsBindingResult.Success(new Dictionary<TKey, TValue>());
            
            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<Dictionary<TKey, TValue>>(settings);

            return SettingsBindingResult.Catch(() => BindInternal(settings));
        }

        private SettingsBindingResult<Dictionary<TKey, TValue>> BindInternal(ISettingsNode settings)
        {
            var results = settings.Children.Select(
                    n => (index: n.Name, key: BindKey(n.Name), value: valueBinder.BindOrDefault(n)))
                .ToList();

            var errors = results.SelectMany(
                    p => p.key.Errors.ForIndex(p.index).Concat(p.value.Errors.ForIndex(p.index)))
                .ToList();
            
            if (errors.Any())
                return SettingsBindingResult.Errors<Dictionary<TKey, TValue>>(errors);
            
            return SettingsBindingResult.Success(results.ToDictionary(p => p.key.Value, p => p.value.Value));
        }

        private SettingsBindingResult<TKey> BindKey(string key)
        {
            var node = new ValueNode(key);

            if (node.IsNullValue(keyBinder))
                return SettingsBindingResult.DictionaryKeyIsNull<TKey>(key);

            return keyBinder.Bind(node);
        }

        SettingsBindingResult<IDictionary<TKey, TValue>> ISafeSettingsBinder<IDictionary<TKey, TValue>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<Dictionary<TKey, TValue>, IDictionary<TKey, TValue>>();

        SettingsBindingResult<IReadOnlyDictionary<TKey, TValue>> ISafeSettingsBinder<IReadOnlyDictionary<TKey, TValue>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>>();
    }
}