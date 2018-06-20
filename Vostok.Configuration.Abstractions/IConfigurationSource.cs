using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Abstractions
{
    /// <summary>
    /// Provides configuration in the form of raw settings trees.
    /// </summary>
    public interface IConfigurationSource
    {
        /// <summary>
        /// <para>Returns the most recent version of settings.</para>
        /// <para>The returned <see cref="ISettingsNode"/> instance is cached, so this method is cheap and can be called freely.</para>
        /// <para>An exception can be thrown if the source is unavailable or contains invalid data, and there is no cached version of settings.</para>
        /// </summary>
        ISettingsNode Get();
        
        /// <summary>
        /// <para>Returns an observable sequence of raw settings.</para>
        /// <list type="bullet">
        ///     <listheader>Subscription rules:</listheader>
        ///     <item><description>New subscribers receive the current value immediately after subscription, even if there is no value (null is returned in this case).</description></item>
        ///     <item><description>Subscribers are notified only when the settings tree has actually changed.</description></item>
        ///     <item><description>If the settings source becomes unavailable, new subscribers still receive the last seen value. Old subscribers do not receive any notifications.</description></item>
        ///     <item><description>All sorts of errors are pushed via <see cref="IObserver{T}.OnError"/></description></item>
        /// </list>
        /// </summary>
        IObservable<ISettingsNode> Observe();
    }
}