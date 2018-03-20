using System;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    /// <summary>
    /// In tests you substitute this one.
    /// Using a per-project extension method you can get rid of generic type on Get.
    /// </summary>
    public interface IConfigurationProvider
    {
        // TODO(krait): ICP decides whether to throw on invalid configs or ignore errors

        /// <summary>
        /// Get binded value from sources set to same type
        /// </summary>
        /// <typeparam name="TSettings">Type of value you need to get</typeparam>
        /// <returns>Combined value</returns>
        TSettings Get<TSettings>();

        // TODO(krait): take ISettings?
        /// <summary>
        /// Get binded value from specified source
        /// </summary>
        /// <typeparam name="TSettings">Type of value you need to get</typeparam>
        /// <param name="source">Source of RawSettings tree</param>
        /// <returns>Value from specified source</returns>
        TSettings Get<TSettings>(IConfigurationSource source);

        /// <summary>
        /// Watches file changes of any of sources by specified type
        /// </summary>
        /// <typeparam name="TSettings">Type of value you need to get</typeparam>
        /// <returns>Event with new combined value</returns>
        IObservable<TSettings> Observe<TSettings>();

        /// <summary>
        /// Watches file changes of specified source only
        /// </summary>
        /// <typeparam name="TSettings">Type of value you need to get</typeparam>
        /// <param name="source">Source of RawSettings tree</param>
        /// <returns>Event with new value from specified source</returns>
        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
}