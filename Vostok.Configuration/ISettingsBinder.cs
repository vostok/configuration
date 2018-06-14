using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration
{
    /// <summary>
    /// Implements binding of <see cref="ISettingsNode"/>s to specific models.
    /// </summary>
    public interface ISettingsBinder
    {
        /// <summary>
        /// <para>Binds the provided <see cref="ISettingsNode"/> instance to type <see cref="TSettings"/>.</para>
        /// <para>An exception will be thrown if the binding fails.</para>
        /// </summary>
        TSettings Bind<TSettings>(ISettingsNode rawSettings);
    }
}