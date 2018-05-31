using System;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Provides recent 
    /// </summary>
    public interface IConfigurationSource : IDisposable
    {
        /// <summary>
        /// <para>Returns the most recent version of settings.</para>
        /// <para>The returned <see cref="IRawSettings"/> instance is cached, so this method is cheap and can be called freely.</para>
        /// </summary>
        IRawSettings Get();

        /// <summary>
        /// <para>Returns an observable sequence of raw settings.</para>
        /// <para>New subscribers receive the current value immediately after subscription.</para>
        /// </summary>
        /// <returns></returns>
        IObservable<IRawSettings> Observe();
    }
}