using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders.Collection
{
    internal class SetBinder<T> :
        ISafeSettingsBinder<HashSet<T>>,
        ISafeSettingsBinder<ISet<T>>
    {
        private readonly ISafeSettingsBinder<T> elementBinder;

        public SetBinder(ISafeSettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public SettingsBindingResult<HashSet<T>> Bind(ISettingsNode settings)
        {
            if (settings.IsNullOrMissing())
                return SettingsBindingResult.Success(new HashSet<T>());

            settings = settings.WrapIfNeeded();

            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<HashSet<T>>(settings);

            return SettingsBindingResult.Catch(() => BindInternal(settings));
        }

        private SettingsBindingResult<HashSet<T>> BindInternal(ISettingsNode settings)
        {
            var results = settings.Children.Select((n, i) => (index: i, value: elementBinder.BindOrDefault(n))).ToList();

            var errors = results.SelectMany(r => r.value.Errors.ForIndex(r.index)).ToList();

            if (errors.Any())
                return SettingsBindingResult.Errors<HashSet<T>>(errors);

            return SettingsBindingResult.Success(new HashSet<T>(results.Select(r => r.value.Value)));
        }

        SettingsBindingResult<ISet<T>> ISafeSettingsBinder<ISet<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<HashSet<T>, ISet<T>>();
    }
}