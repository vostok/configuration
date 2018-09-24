using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Binders
{
    internal class NullableBinder<T> : ISettingsBinder<T?>
        where T : struct
    {
        private readonly ISettingsBinder<T> elementBinder;

        public NullableBinder(ISettingsBinder<T> elementBinder) =>
            this.elementBinder = elementBinder;

        public T? Bind(ISettingsNode settings)
        {
            SettingsNode.CheckSettings(settings, false);
            return settings.Value == null ? (T?) null : elementBinder.Bind(settings);
        }
    }
}