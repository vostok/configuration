using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration
{
    /// <summary>
    /// Settings for <see cref="ConfigurationProvider"/>.
    /// </summary>
    [PublicAPI]
    public class ConfigurationProviderSettings
    {
        /// <summary>
        /// In case OnError() notification is received from a <see cref="IConfigurationSource"/>, the <see cref="ConfigurationProvider"/> will wait for <see cref="SourceRetryCooldown"/> and then restart observing that source.
        /// </summary>
        public TimeSpan SourceRetryCooldown = TimeSpan.FromSeconds(10);

        /// <summary>
        /// In case an exception is encountered during a <see cref="ConfigurationProvider.Get{T}()"/> call, <see cref="ConfigurationProvider"/> will wait for <see cref="ValueRetryCooldown"/> between resubscription attempts to underlying observable in order to heal.
        /// </summary>
        public TimeSpan ValueRetryCooldown = TimeSpan.FromSeconds(3);

        /// <summary>
        /// <para>Use this to specify a custom implementation of <see cref="ISettingsBinder"/>.</para>
        /// <para><see cref="DefaultSettingsBinder"/> will be used by default.</para>
        /// </summary>
        [CanBeNull]
        public ISettingsBinder Binder { get; set; }

        /// <summary>
        /// <para>An optional callback that receives all errors encountered by <see cref="ConfigurationProvider.Observe{TSettings}()"/> and <see cref="ConfigurationProvider.Observe{TSettings}(IConfigurationSource)"/>.</para>
        /// <para>The callback will not be called when using <see cref="ConfigurationProvider.ObserveWithErrors{TSettings}()"/> or <see cref="ConfigurationProvider.ObserveWithErrors{TSettings}(IConfigurationSource)"/>.</para>
        /// <para>A common use case for this callback is error logging.</para>
        /// </summary>
        [CanBeNull]
        public Action<Exception> ErrorCallback { get; set; }

        /// <summary>
        /// <para>An optional callback that receives all new unique non-null settings instances produced by <see cref="ConfigurationProvider"/>s with these settings alongside with corresponding sources.</para>
        /// <para>Useful for implementing logging of app settings.</para>
        /// <para>Applies to all kinds of methods in <see cref="ConfigurationProvider"/>.</para>
        /// </summary>
        [CanBeNull]
        public Action<object, IConfigurationSource> SettingsCallback { get; set; }

        /// <summary>
        /// Specifies how many <see cref="IConfigurationSource"/>s passed to <see cref="ConfigurationProvider.Get{TSettings}(IConfigurationSource)"/>, <see cref="ConfigurationProvider.Observe{TSettings}(IConfigurationSource)"/> or <see cref="ConfigurationProvider.ObserveWithErrors{TSettings}(IConfigurationSource)"/> method a <see cref="ConfigurationProvider"/> should cache.
        /// </summary>
        public int MaxSourceCacheSize { get; set; } = 50;
    }
}