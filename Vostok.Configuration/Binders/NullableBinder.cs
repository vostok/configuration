using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class NullableBinder<T> : ISettingsBinder<T?>
        where T : struct
    {
        private readonly ISettingsBinder<T> valueBinder;

        public NullableBinder(ISettingsBinder<T> valueBinder) =>
            this.valueBinder = valueBinder;

        public T? Bind(ISettingsNode settings) =>
            settings.Value == null ? (T?)null : valueBinder.Bind(settings);
    }
}