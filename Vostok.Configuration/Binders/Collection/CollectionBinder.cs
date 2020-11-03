using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Parsers;

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

            if (settings is ValueNode && typeof(TValue) == typeof(byte))
                return BindBase64ByteArray(settings);

            settings = settings.WrapIfNeeded();

            if (!(settings is ArrayNode) && !(settings is ObjectNode))
                return SettingsBindingResult.NodeTypeMismatch<TCollection>(settings);

            return SettingsBindingResult.Catch(() => BindInternal(settings));
        }

        private SettingsBindingResult<TCollection> BindBase64ByteArray(ISettingsNode settings)
        {
            if (ByteArrayParser.TryParse(settings.Value, out var result))
                return SettingsBindingResult.Success(CreateCollection(result.Cast<TValue>()));

            return SettingsBindingResult.Error<TCollection>("Failed to parse base64 string.");
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