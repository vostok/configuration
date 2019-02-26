using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class IdentityBinder : ISafeSettingsBinder<ISettingsNode>
    {
        public SettingsBindingResult<ISettingsNode> Bind(ISettingsNode rawSettings) =>
            SettingsBindingResult.Success(rawSettings);
    }
}