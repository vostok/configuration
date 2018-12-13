using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    internal class ConfigurationGetter_Tests
    {
        private ConfigurationGetter configurationGetter;
        private IConfigurationObservable configurationObservable;
        private ICachingBinder cachingBinder;
        private ISourceDataCache sourceDataCache;
        private ITaskSourceFactory taskSourceFactory;
        private ITaskSource<object> taskSource;
        private object settings;
        private IConfigurationSource source;

        [SetUp]
        public void SetUp()
        {
            source = Substitute.For<IConfigurationSource>();
            configurationObservable = Substitute.For<IConfigurationObservable>();
            cachingBinder = Substitute.For<ICachingBinder>();
            sourceDataCache = Substitute.For<ISourceDataCache>();
            sourceDataCache.GetLimitedCacheItem<object>(source).Returns(new SourceCacheItem<object>());
            taskSourceFactory = Substitute.For<ITaskSourceFactory>();
            
            settings = new object();
            
            taskSource = Substitute.For<ITaskSource<object>>();
            taskSource.Get(() => null).ReturnsForAnyArgs(settings);
            taskSourceFactory.Create<object>().Returns(taskSource);
            
            configurationGetter = new ConfigurationGetter(configurationObservable, cachingBinder, sourceDataCache, taskSourceFactory);
        }

        [Test]
        public void Should_use_taskSource_when_preconfigured_source()
        {
            configurationGetter.Get<object>().Should().BeSameAs(settings);
        }

        [Test]
        public void Should_cache_taskSource_by_type()
        {
            configurationGetter.Get<object>();
            configurationGetter.Get<object>().Should().BeSameAs(settings);
            
            configurationGetter.Get<int>();
            
            taskSourceFactory.Create<object>().Received(1);
            taskSourceFactory.Create<int>().Received(1);
        }

        [Test]
        public void Should_use_configurationObservable_when_preconfigured_source()
        {
            configurationGetter.Get<object>();

            configurationObservable.Observe<object>().Received(1);
        }

        [Test]
        public void Should_use_source_Get_when_custom_source()
        {
            configurationGetter.Get<object>(source);

            source.Get().Received(1);
        }

        [Test]
        public void Should_use_limited_cache_when_custom_source()
        {
            configurationGetter.Get<object>(source);

            sourceDataCache.GetLimitedCacheItem<object>(source).Received(1);
            sourceDataCache.GetPersistentCacheItem<object>(source).DidNotReceiveWithAnyArgs();
        }

        [Test]
        public void Should_use_cachingBinder_when_custom_source()
        {
            var cacheItem = new SourceCacheItem<object>();
            sourceDataCache.GetLimitedCacheItem<object>(source).Returns(cacheItem);

            var settingsNode = Substitute.For<ISettingsNode>();

            source.Get().Returns(settingsNode);
            
            configurationGetter.Get<object>(source);

            cachingBinder.Bind(settingsNode, cacheItem.BindingCacheItem).Received(1);
        }

        [Test]
        public void Should_save_successfully_bound_value_to_lastValue_cache_when_custom_source()
        {
            var lastError = new Exception();
            var cacheItem = new SourceCacheItem<object> {LastValue = (new object(), lastError)};
            sourceDataCache.GetLimitedCacheItem<object>(source).Returns(cacheItem);
            
            cachingBinder.Bind(null, Arg.Any<CachingBinder.BindingCacheItem<object>>()).Returns(settings);

            configurationGetter.Get<object>(source).Should().BeSameAs(settings);

            cacheItem.LastValue.Should().Be((settings, lastError));
        }
    }
}