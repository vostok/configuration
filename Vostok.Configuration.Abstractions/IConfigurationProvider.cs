using System;

namespace Vostok.Configuration.Abstractions
{
    /// <summary>
    /// Provides settings for your application, fresh and warm.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// <para>Returns the most recent value of <see cref="TSettings"/> from precofigured configuration sources.</para>
        /// <para>The settings instance is cached, so this method is not time-consuming and can be called freely.</para>
        /// <para>An exception will be thrown if there is no source configured for <see cref="TSettings"/>.</para>
        /// <para>Other types of errors are handled as specified for the current <see cref="ConfigurationProvider"/> instance.</para>
        /// </summary>
        TSettings Get<TSettings>();

        /// <summary>
        /// <para>Returns the most up-to-date value of <see cref="TSettings"/> from the given <paramref name="source"/>.</para>
        /// <para>Internal caches of <see cref="IConfigurationProvider"/> are not used by this method.</para>
        /// <para>All types of errors are handled as specified for the current <see cref="ConfigurationProvider"/> instance.</para>
        /// </summary>
        TSettings Get<TSettings>(IConfigurationSource source);

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from its corresponding preconfigured source.</para>
        /// <para>The current value is observed immediately after subscription.</para>
        /// <para>An exception will be thrown if there is no source configured for <see cref="TSettings"/>.</para>
        /// <para>Other types of errors are handled as specified for the current <see cref="ConfigurationProvider"/> instance.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>();

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from the given <paramref name="source"/>.</para>
        /// <para>The current value is observed immediately after subscription.</para>
        /// <para>All types of errors are handled as specified for the current <see cref="ConfigurationProvider"/> instance.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
}