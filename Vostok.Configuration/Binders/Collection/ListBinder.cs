using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
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
            if (settings.IsNullOrMissing())
                return SettingsBindingResult.Success(new List<T>());
            
            settings = settings.WrapIfNeeded();
            
            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<List<T>>(settings);

            return SettingsBindingResult.Catch(() => BindInternal(settings));
        }

        private SettingsBindingResult<List<T>> BindInternal(ISettingsNode settings)
        {
            var results = settings.Children.Select((n, i) => (index: i, value: elementBinder.BindOrDefault(n))).ToList();

            var errors = results.SelectMany(r => r.value.Errors.ForIndex(r.index)).ToList();
            
            if (errors.Any())
                return SettingsBindingResult.Errors<List<T>>(errors);

            return SettingsBindingResult.Success(results.Select(r => r.value.Value).ToList());
        }

        SettingsBindingResult<IList<T>> ISafeSettingsBinder<IList<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, IList<T>>();

        SettingsBindingResult<ICollection<T>> ISafeSettingsBinder<ICollection<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<List<T>, ICollection<T>>();
    }
}