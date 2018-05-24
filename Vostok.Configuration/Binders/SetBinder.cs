using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders
{
    internal class SetBinder<T> :
        ISettingsBinder<HashSet<T>>,
        ISettingsBinder<ISet<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public SetBinder(ISettingsBinder<T> kelementBinder)
        {
            elementBinder = kelementBinder;
        }

        public HashSet<T> Bind(IRawSettings settings)
        {
            RawSettings.CheckSettings(settings);
            return new HashSet<T>(
                settings.Children.Select(n => elementBinder.Bind(n)));
        }

        ISet<T> ISettingsBinder<ISet<T>>.Bind(IRawSettings settings) => Bind(settings);
    }
}