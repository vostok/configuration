using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal interface ISafeSettingsBinder<TSettings>
    {
        [NotNull]
        SettingsBindingResult<TSettings> Bind([CanBeNull] ISettingsNode rawSettings);
    }
}