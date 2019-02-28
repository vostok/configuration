using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Extensions.ConfigurationProvider
{
    /// <summary>
    /// Contains extensions of IConfigurationProvider
    /// </summary>
    [PublicAPI]
    public static class ConfigurationProviderExtensions
    {
        private static readonly ConcurrentDictionary<(Type, IConfigurationSource), Lazy<ConfigUpdater>> updatersCache =
            new ConcurrentDictionary<(Type, IConfigurationSource), Lazy<ConfigUpdater>>();

        /// <summary>
        /// Creates instance of <typeparamref name="TConfig"/> with hot properties
        /// and fields if <typeparamref name="TConfig"/> is class
        /// </summary>
        /// <param name="provider">Instance of IConfigurationProvider</param>
        /// <param name="source">Instance of IConfigurationSource</param>
        /// <typeparam name="TConfig">Interface or class with properties.</typeparam>
        /// <returns>Instance of <typeparamref name="TConfig"/></returns>
        public static TConfig GetHot<TConfig>(this IConfigurationProvider provider, IConfigurationSource source) =>
            (TConfig)provider.GetHot(typeof(TConfig), source);

        /// <summary>
        /// Creates instance of <paramref name="type"/> with hot properties
        /// and fields if <paramref name="type"/> is class
        /// </summary>
        /// <param name="provider">Instance of IConfigurationProvider</param>
        /// <param name="type">Interface or class with properties.</param>
        /// <param name="source">Instance of IConfigurationSource</param>
        /// <returns>Instance of <paramref name="type"/></returns>
        public static object GetHot(this IConfigurationProvider provider, Type type, IConfigurationSource source)
        {
            var lazyUpdater = updatersCache.GetOrAdd((type, source), key => CreateLazyConfigUpdater(provider, key.Item1, source));
            return lazyUpdater.Value.Config;
        }

        private static Lazy<ConfigUpdater> CreateLazyConfigUpdater(IConfigurationProvider provider, Type type, IConfigurationSource source) =>
            new Lazy<ConfigUpdater>(
                () =>
                {
                    ValidateConfigType(type);
                    var typeImpl = DynamicTypesHelper.ImplementTypeIfNeeded(type);
                    var initialConfig = GetInitialConfig(provider, typeImpl, source);
                    FillFieldsAndPropsWithInterfaceType(initialConfig, provider, source);
                    var observable = (IObservable<object>)GetConfigObservable(provider, typeImpl, source);
                    return new ConfigUpdater(initialConfig, observable);
                });

        private static void ValidateConfigType(Type configType)
        {
            if (!configType.IsInterface && (!configType.IsClass || configType.IsAbstract))
                throw new Exception($"Unsupported type {configType.FullName}. Only interfaces and not abstract classes are supported.");
        }

        private static void FillFieldsAndPropsWithInterfaceType(object config, IConfigurationProvider provider, IConfigurationSource source)
        {
            foreach (var fieldInfo in config.GetType().GetFields().Where(pi => pi.FieldType.IsInterface))
                fieldInfo.SetValue(config, provider.GetHot(fieldInfo.FieldType, new ScopedSource(source, fieldInfo.Name)));

            foreach (var propertyInfo in config.GetType().GetProperties().Where(pi => pi.PropertyType.IsInterface))
                propertyInfo.ForceSetValue(config, provider.GetHot(propertyInfo.PropertyType, new ScopedSource(source, propertyInfo.Name)));
        }

        private static object GetInitialConfig(IConfigurationProvider provider, Type configTypeImpl, IConfigurationSource source) =>
            CallGenericMethod(provider, configTypeImpl, nameof(IConfigurationProvider.Get), source);

        private static object GetConfigObservable(IConfigurationProvider provider, Type configTypeImpl, IConfigurationSource source) =>
            CallGenericMethod(provider, configTypeImpl, nameof(IConfigurationProvider.Observe), source);

        private static object CallGenericMethod(IConfigurationProvider provider, Type configTypeImpl, string methodName, IConfigurationSource source)
        {
            var argumentTypes = source == null ? Type.EmptyTypes : new[] {typeof(IConfigurationSource)};
            var methodInfo = typeof(IConfigurationProvider).GetMethod(methodName, argumentTypes);
            if (methodInfo == null) throw new NullReferenceException(methodName);
            var method = methodInfo.MakeGenericMethod(configTypeImpl);
            return method.Invoke(provider, source == null ? new object[0] : new object[] {source});
        }
    }
}