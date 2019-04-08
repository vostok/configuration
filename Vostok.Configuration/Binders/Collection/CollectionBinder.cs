using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders.Collection
{
    internal abstract class CollectionBinder<TCollection, TValue>
        where TCollection : IEnumerable<TValue>
    {
        private readonly ISafeSettingsBinder<TValue> elementBinder;

        protected CollectionBinder(ISafeSettingsBinder<TValue> elementBinder) =>
            this.elementBinder = elementBinder;

        public SettingsBindingResult<TCollection> Bind(ISettingsNode settings)
        {
            if (settings.IsNullOrMissing())
                return SettingsBindingResult.Success(CreateCollection(Enumerable.Empty<TValue>()));

            settings = settings.WrapIfNeeded();

            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<TCollection>(settings);

            return SettingsBindingResult.Catch(() => BindInternal(settings));
        }

        protected abstract TCollection CreateCollection(IEnumerable<TValue> elements);

        private SettingsBindingResult<TCollection> BindInternal(ISettingsNode settings)
        {
            var results = settings.Children.Select((n, i) => (index: i, value: elementBinder.BindOrDefault(n))).ToList();

            var errors = results.SelectMany(r => r.value.Errors.ForIndex(r.index)).ToList();

            if (errors.Any())
                return SettingsBindingResult.Errors<TCollection>(errors);

            return SettingsBindingResult.Success(CreateCollection(results.Select(r => r.value.Value)));
        }
    }
}