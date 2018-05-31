using System;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    // TODO(krait): On error
    /// <summary>
    /// Provides settings for your application, fresh and warm.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// <para>Returns the most up-to-date value of <see cref="TSettings"/> from precofigured configuration sources.</para>
        /// <para>The settings instance is cached, so this method is not time-consuming and can be called freely.</para>
        /// </summary>
        TSettings Get<TSettings>();

        /// <summary>
        /// <para>Returns the most up-to-date value of <see cref="TSettings"/> from the given <paramref name="source"/>.</para>
        /// <para>Internal caches of <see cref="IConfigurationProvider"/> are not used by this method.</para>
        /// </summary>
        TSettings Get<TSettings>(IConfigurationSource source);

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from its corresponding preconfigured source.</para>
        /// <para>The current value is observed immediately after subscription.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>();

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from the given <paramref name="source"/>.</para>
        /// <para>The current value is observed immediately after subscription.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
}