using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Binders
{
    public interface ISettingsBinder<out T>
    {
        T Bind(ISettingsNode rawSettings);
    }
}