using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Extensions
{
    [PublicAPI]
    public static class IConfigurationProviderExtensions
    {
        /// <summary>
        /// <para>Creates an instance of <typeparamref name="TConfig"/> interface with "hot" properties based on given <paramref name="provider"/> and <paramref name="source"/>.</para>
        /// </summary>
        /// <param name="provider">An instance of <see cref="IConfigurationProvider"/></param>
        /// <param name="source">An instance of <see cref="IConfigurationSource"/></param>
        /// <typeparam name="TConfig">An interface type</typeparam>
        /// <returns>A tuple of the instance of <typeparamref name="TConfig"/> and a disposable result of call IObservable&lt;TConfig&gt;.Subscribe()</returns>
        /// <exception cref="ArgumentException">Provided <typeparamref name="TConfig"/> type is not an interface.</exception>
        /// <remarks>
        /// <para>The properties may not be consistent with each other at any given instant.</para>
        /// <para>This means that accessing multiple properties of the returned object may yield results from different versions of configuration.</para>
        /// <para>This incosistency may be avoided by using nested configuration classes/interfaces:</para>
        /// <code>
        /// interface IConfig
        /// {
        ///     // Observing SubConfig1 and SubConfig2 may yield results from different configurations due to race condition.
        ///     ISubConfig1 SubConfig1 { get; }
        ///     ISubConfig2 SubConfig2 { get; }
        /// }
        /// interface ISubConfig1
        /// {
        ///     int MaxCount { get; }
        ///     bool EnableFeature { get; }
        /// }
        /// class App
        /// {
        ///     public void Run(IConfigurationProvider provider, IConfigurationSource source)
        ///     {
        ///         var config = provider.GetHot&lt;IConfig&gt;(source);
        ///         var subConfig1 = config.SubConfig1;
        ///         // Consistently observe all properties of subConfig1.
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static (TConfig, IDisposable) GetHot<TConfig>(this IConfigurationProvider provider, IConfigurationSource source)
            where TConfig : class => ((TConfig, IDisposable))provider.GetHot(typeof(TConfig), source);

        private static (object, IDisposable) GetHot(this IConfigurationProvider provider, Type type, IConfigurationSource source)
        {
            if (!type.IsInterface)
                throw new ArgumentException($"Unsupported type '{type.FullName}'. Only interfaces are supported.");

            var wrapperType = DynamicTypesHelper.ImplementWrapperType(type);
            var wrapper = Activator.CreateInstance(wrapperType);

            var initialConfig = GetInitialConfig(provider, type, source);
            var observable = (IObservable<object>)GetConfigObservable(provider, type, source);

            DynamicTypesHelper.SetCurrentInstance(wrapper, initialConfig);
            var subscription = observable.Subscribe(nextInstance => DynamicTypesHelper.SetCurrentInstance(wrapper, nextInstance));

            return (wrapper, subscription);
        }

        private static object GetInitialConfig(IConfigurationProvider provider, Type configTypeImpl, IConfigurationSource source) =>
            CallGenericMethod(provider, configTypeImpl, nameof(IConfigurationProvider.Get), source);

        private static object GetConfigObservable(IConfigurationProvider provider, Type configTypeImpl, IConfigurationSource source) =>
            CallGenericMethod(provider, configTypeImpl, nameof(IConfigurationProvider.Observe), source);

        private static object CallGenericMethod(IConfigurationProvider provider, Type configTypeImpl, string methodName, IConfigurationSource source)
        {
            var argumentTypes = source == null ? Type.EmptyTypes : new[] {typeof(IConfigurationSource)};
            var method = typeof(IConfigurationProvider).GetMethod(methodName, argumentTypes);
            if (method == null) throw new ArgumentException($"Can't find method by name {methodName}");
            var genericMethod = method.MakeGenericMethod(configTypeImpl);
            return genericMethod.Invoke(provider, source == null ? new object[0] : new object[] {source});
        }
    }
}