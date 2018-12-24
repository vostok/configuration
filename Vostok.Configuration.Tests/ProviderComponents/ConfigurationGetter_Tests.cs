using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Cache;
using Vostok.Configuration.ProviderComponents;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Tests.ProviderComponents
{
    [TestFixture]
    internal class ConfigurationGetter_Tests
    {
        private ConfigurationGetter configurationGetter;
        private IConfigurationObservable configurationObservable;
        private ISourceDataCache sourceDataCache;
        private ITaskSourceFactory taskSourceFactory;
        private ITaskSource<object> taskSource;
        private object settings;
        private IConfigurationSource source;
        private Func<Type, IConfigurationSource> sourceProvider;

        [SetUp]
        public void SetUp()
        {
            sourceProvider = Substitute.For<Func<Type, IConfigurationSource>>();
            source = Substitute.For<IConfigurationSource>();
            sourceProvider.Invoke(typeof(object)).Returns(source);
            configurationObservable = Substitute.For<IConfigurationObservable>();
            sourceDataCache = Substitute.ForPartsOf<SourceDataCache>(10);
            taskSourceFactory = Substitute.For<ITaskSourceFactory>();

            settings = new object();

            taskSource = Substitute.For<ITaskSource<object>>();
            taskSource.Get().Returns(settings);
            taskSourceFactory.Create<object>(default).ReturnsForAnyArgs(taskSource);
            taskSourceFactory.WhenForAnyArgs(f => f.Create<object>(default))
                .Do(callInfo => callInfo.ArgAt<Func<IObservable<object>>>(0).Invoke());

            configurationGetter = new ConfigurationGetter(sourceProvider, configurationObservable, sourceDataCache, taskSourceFactory);
        }

        [Test]
        public void Should_use_taskSource([Values] bool customSource)
        {
            Get<object>(customSource).Should().BeSameAs(settings);
        }

        [Test]
        public void Should_cache_taskSource_by_type_and_source([Values] bool customSource)
        {
            Get<object>(customSource);
            Get<object>(customSource).Should().BeSameAs(settings);

            Get<int>(customSource);

            taskSourceFactory.ReceivedWithAnyArgs(1).Create<object>(default);
            taskSourceFactory.ReceivedWithAnyArgs(1).Create<int>(default);
        }

        [Test]
        public void Should_use_persistent_cache_when_preconfigured_source()
        {
            configurationGetter.Get<object>();

            sourceDataCache.ReceivedWithAnyArgs().GetPersistentCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetLimitedCacheItem<object>(default);
        }

        [Test]
        public void Should_use_limited_cache_when_custom_source()
        {
            configurationGetter.Get<object>(source);

            sourceDataCache.ReceivedWithAnyArgs().GetLimitedCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetPersistentCacheItem<object>(default);
        }

        [Test]
        public void Should_use_configurationObservable_when_preconfigured_source()
        {
            configurationGetter.Get<object>();

            configurationObservable.Received(1).Observe<object>();
        }

        [Test]
        public void Should_use_configurationObservable_when_custom_source()
        {
            configurationGetter.Get<object>(source);

            configurationObservable.Received(1).Observe<object>(source);
        }
        
        private T Get<T>(bool customSource)
        {
            return customSource
                ? configurationGetter.Get<T>(source)
                : configurationGetter.Get<T>();
        }
    }
}