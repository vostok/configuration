using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class IdentityBinder : ISafeSettingsBinder<ISettingsNode>, ISettingsBinder
    {
        public SettingsBindingResult<ISettingsNode> Bind(ISettingsNode rawSettings) =>
            SettingsBindingResult.Success(rawSettings);

        public TSettings Bind<TSettings>(ISettingsNode rawSettings) 
            => (TSettings) rawSettings;
    }
}