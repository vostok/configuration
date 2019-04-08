using System.Collections.Generic;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders.Collection
{
    internal class SetBinder<T> : CollectionBinder<HashSet<T>, T>,
        ISafeSettingsBinder<HashSet<T>>,
        ISafeSettingsBinder<ISet<T>>
    {
        public SetBinder(ISafeSettingsBinder<T> elementBinder)
            : base(elementBinder)
        {
        }

        protected override HashSet<T> CreateCollection(IEnumerable<T> elements) =>
            new HashSet<T>(elements);

        SettingsBindingResult<ISet<T>> ISafeSettingsBinder<ISet<T>>.Bind(ISettingsNode settings) =>
            Bind(settings).Convert<HashSet<T>, ISet<T>>();
    }
}