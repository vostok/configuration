using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Cache;

namespace Vostok.Configuration.Tests.Cache
{
    [TestFixture]
    internal class CachingBinder_Tests
    {
        private ISettingsBinder binder;
        private CachingBinder cachingBinder;
        private ISettingsNode node;
        private object settings;

        [SetUp]
        public void SetUp()
        {
            binder = Substitute.For<ISettingsBinder>();
            cachingBinder = new CachingBinder(binder);

            node = new ValueNode("value");
            settings = new object();
        }

        [Test]
        public void Should_use_underlying_binder_when_no_cached_value()
        {
            binder.Bind<object>(node).Returns(settings);
            
            cachingBinder.Bind(node, new SourceCacheItem<object>()).Should().Be(settings);
        }

        [Test]
        public void Should_use_underlying_binder_when_different_cached_value()
        {
            binder.Bind<object>(node).Returns(settings);

            var cacheItem = new SourceCacheItem<object>
            {
                BindingCacheValue = new BindingCacheValue<object>(Substitute.For<ISettingsNode>(), new object())
            };
            
            cachingBinder.Bind(node, cacheItem).Should().Be(settings);
        }

        [Test]
        public void Should_throw_when_underlying_binder_throws()
        {
            var error = new FormatException();
            binder.Bind<object>(node).Throws(error);

            new Action(() => cachingBinder.Bind(node, new SourceCacheItem<object>())).Should().Throw<FormatException>();
        }

        [Test]
        public void Should_save_binding_result_to_cacheItem_when_binding_succeeds()
        {
            binder.Bind<object>(node).Returns(settings);

            var cacheItem = new SourceCacheItem<object>();
            cachingBinder.Bind(node, cacheItem);
            
            cacheItem.BindingCacheValue.Should().BeEquivalentTo(new BindingCacheValue<object>(node, settings));
        }

        [Test]
        public void Should_save_binding_result_to_cacheItem_when_binding_fails()
        {
            var error = new Exception();
            binder.Bind<object>(node).Throws(error);

            var cacheItem = new SourceCacheItem<object>();
            try
            {
                cachingBinder.Bind(node, cacheItem);
            }
            catch (Exception)
            {
                // ignored
            }

            cacheItem.BindingCacheValue.Should().BeEquivalentTo(new BindingCacheValue<object>(node, error));
        }

        [Test]
        public void Should_return_cached_settings_when_node_equals_to_cached()
        {
            var cacheItem = new SourceCacheItem<object>
            {
                BindingCacheValue = new BindingCacheValue<object>(node, settings)
            };
            
            cachingBinder.Bind(new ValueNode("value"), cacheItem).Should().Be(settings);

            binder.DidNotReceiveWithAnyArgs().Bind<object>(default);
        }
        
        [Test]
        public void Should_rethrow_cached_error_when_node_equals_to_cached()
        {
            var error = new FormatException();
            var cacheItem = new SourceCacheItem<object>
            {
                BindingCacheValue = new BindingCacheValue<object>(node, error)
            };

            new Action(() => cachingBinder.Bind(new ValueNode("value"), cacheItem)).Should().Throw<FormatException>();

            binder.DidNotReceiveWithAnyArgs().Bind<object>(default);
        }
    }
}