using System;

namespace Vostok.Configuration.Abstractions
{
    // TODO(krait): Split docs between this and the actual implementation.
    /// <summary>
    /// Provides settings for your application, fresh and warm.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// <para>Returns the most recent value of <see cref="TSettings"/> from preconfigured configuration sources.</para>
        /// <para>The settings instance should be cached by implementations, so this method is cheap and can be called freely.</para>
        /// <para>An exception will be thrown if there is no source configured for <see cref="TSettings"/>.</para>
        /// </summary>
        TSettings Get<TSettings>();

        /// <summary>
        /// <para>Returns the most recent value of <see cref="TSettings"/> from the given <paramref name="source"/>.</para>
        /// <para>Values returned by this method are also stored in a small-capacity cache.</para>
        /// </summary>
        TSettings Get<TSettings>(IConfigurationSource source);

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from its corresponding preconfigured source.</para>
        /// <para>If a current value is available, it is observed immediately after subscription.</para>
        /// <para>Other types of errors are handled as specified for the current <see cref="ConfigurationProvider"/> instance.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>();

        /// <summary>
        /// <para>Returns an <see cref="IObservable{T}"/> that receives the new value of <see cref="TSettings"/> each time it is updated from the given <paramref name="source"/>.</para>
        /// <para>If a current value is available, it is observed immediately after subscription.</para>
        /// </summary>
        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
}