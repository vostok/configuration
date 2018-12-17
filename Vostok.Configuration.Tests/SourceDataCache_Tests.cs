using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    internal class SourceDataCache_Tests
    {
        private SourceDataCache cache;
        private IConfigurationSource[] sources;

        [SetUp]
        public void SetUp()
        {
            sources = Enumerable.Range(0, 10).Select(_ => Substitute.For<IConfigurationSource>()).ToArray();
            cache = new SourceDataCache(2);
        }
        
        [Test]
        public void Should_cache_up_to_limit_items_when_limited_cache()
        {
            GetItemsFromCache(2).Should().Equal(GetItemsFromCache(2));
        }

        [Test]
        public void Should_dispose_items_when_remove_by_overflow_from_limited_cache()
        {
            var items = GetItemsFromCache(3);
            
            items[0].IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Should_cache_persistent_when_persistent_cache()
        {
            GetItemsFromCache(10, false).Should().Equal(GetItemsFromCache(10, false));
        }

        [Test]
        public void Should_move_items_from_limited_to_persistent_cache_when_same_key()
        {
            var item = cache.GetLimitedCacheItem<int>(sources[0]);
            cache.GetLimitedCacheItem<int>(sources[0]).Should().BeSameAs(item);
            
            cache.GetPersistentCacheItem<int>(sources[0]).Should().BeSameAs(item);
            cache.GetLimitedCacheItem<int>(sources[0]).Should().NotBe(item);
        }

        private SourceCacheItem<int>[] GetItemsFromCache(int count, bool limited = true)
        {
            return sources.Take(count).Select(source => limited
                ? cache.GetLimitedCacheItem<int>(source)
                : cache.GetPersistentCacheItem<int>(source)).ToArray();
        }
    }
}