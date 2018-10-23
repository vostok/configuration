using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Collection
{
    internal class DictionaryBinder<T1, T2> :
        ISettingsBinder<Dictionary<T1, T2>>,
        ISettingsBinder<IDictionary<T1, T2>>,
        ISettingsBinder<IReadOnlyDictionary<T1, T2>>
    {
        private readonly ISettingsBinder<T1> keyBinder;
        private readonly ISettingsBinder<T2> valueBinder;

        public DictionaryBinder(ISettingsBinder<T1> keyBinder, ISettingsBinder<T2> valueBinder)
        {
            this.keyBinder = keyBinder;
            this.valueBinder = valueBinder;
        }

        public Dictionary<T1, T2> Bind(ISettingsNode settings) =>
            settings.Children.ToDictionary(n => keyBinder.Bind(new ValueNode(n.Name)), n => valueBinder.Bind(n));

        IDictionary<T1, T2> ISettingsBinder<IDictionary<T1, T2>>.Bind(ISettingsNode settings) => Bind(settings);

        IReadOnlyDictionary<T1, T2> ISettingsBinder<IReadOnlyDictionary<T1, T2>>.Bind(ISettingsNode settings) => Bind(settings);
    }
}