using System;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// File converter to RawSettings tree
    /// </summary>
    public interface IConfigurationSource: IDisposable
    {
        /// <summary>
        /// Converts file
        /// </summary>
        /// <returns>RawSettings tree</returns>
        RawSettings Get();

        /// <summary>
        /// Watches file changes
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        IObservable<RawSettings> Observe();
    }
}