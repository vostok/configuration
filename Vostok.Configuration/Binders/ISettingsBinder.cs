using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    /// <summary>
    /// Implements binding of <see cref="ISettingsNode"/>s to specific models.
    /// </summary>
    [PublicAPI]
    public interface ISettingsBinder<TSettings>
    {
        /// <summary>
        /// <para>Binds the provided <see cref="ISettingsNode"/> instance to type <typeparamref name="TSettings"/>.</para>
        /// <para>An exception will be thrown if the binding fails.</para>
        /// </summary>
        [NotNull]
        SettingsBindingResult<TSettings> Bind([NotNull] ISettingsNode rawSettings);
    }
}