using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Provider
{
    [TestFixture]
    internal class ConfigurationWithErrorsObservable_Tests
    {
        private Func<Type, IConfigurationSource> sourceProvider;
        private IObservableBinder observableBinder;
        private ISourceDataCache sourceDataCache;
        private ConfigurationWithErrorsObservable configurationWithErrorsObservable;
        private IConfigurationSource source;
        private IObservable<(ISettingsNode, Exception)> sourceObservable;

        [SetUp]
        public void SetUp()
        {
            sourceProvider = Substitute.For<Func<Type, IConfigurationSource>>();
            observableBinder = Substitute.For<IObservableBinder>();
            sourceDataCache = Substitute.For<ISourceDataCache>();
            
            source = Substitute.For<IConfigurationSource>();
            sourceObservable = Substitute.For<IObservable<(ISettingsNode, Exception)>>();
            source.Observe().Returns(sourceObservable);
            sourceProvider.Invoke(typeof(object)).Returns(source);
            
            configurationWithErrorsObservable = new ConfigurationWithErrorsObservable(sourceProvider, observableBinder, sourceDataCache);
            observableBinder.WhenForAnyArgs(b => b.SelectBound<object>(default, default))
                .Do(callInfo => callInfo.ArgAt<Func<SourceCacheItem<object>>>(1).Invoke());
        }
        
        [Test]
        public void Should_use_sourceProvider_to_obtain_preconfigured_source()
        {
            configurationWithErrorsObservable.ObserveWithErrors<object>();

            sourceProvider.Received(1).Invoke(typeof(object));
        }

        [Test]
        public void Should_use_persistent_cache_for_preconfigured_source()
        {
            configurationWithErrorsObservable.ObserveWithErrors<object>();
            
            sourceDataCache.ReceivedWithAnyArgs(1).GetPersistentCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetLimitedCacheItem<object>(default);
        }

        [Test]
        public void Should_use_limited_cache_for_custom_source()
        {
            configurationWithErrorsObservable.ObserveWithErrors<object>(source);
            
            sourceDataCache.ReceivedWithAnyArgs(1).GetLimitedCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetPersistentCacheItem<object>(default);
        }

        [Test]
        public void Should_use_observable_from_source([Values] bool customSource)
        {
            if (customSource)
                configurationWithErrorsObservable.ObserveWithErrors<object>(source);
            else
                configurationWithErrorsObservable.ObserveWithErrors<object>();

            observableBinder.Received(1).SelectBound(sourceObservable, Arg.Any<Func<SourceCacheItem<object>>>());
        }
    }
}