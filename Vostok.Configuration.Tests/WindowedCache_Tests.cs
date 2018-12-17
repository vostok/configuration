using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    internal class WindowedCache_Tests
    {
        private WindowedCache<string, int> cache;
        private Action<KeyValuePair<string, int>> onRemove;

        [SetUp]
        public void SetUp()
        {
            onRemove = Substitute.For<Action<KeyValuePair<string, int>>>();
            cache = new WindowedCache<string, int>(2, onRemove);
        }
        
        [Test]
        public void Should_keep_up_to_capacity_items()
        {
            cache.GetOrAdd("key1", _ => 1);
            cache.GetOrAdd("key2", _ => 2);

            AssertHasValue("key1", 1);
            AssertHasValue("key2", 2);
        }

        [Test]
        public void Should_remove_most_recently_added_items_when_all_capacity_filled()
        {   
            for (var i = 1; i <= 3; i++)
                cache.GetOrAdd($"key{i}", _ => i);

            cache.TryGetValue("key1", out _).Should().BeFalse();
            AssertHasValue("key2", 2);
            AssertHasValue("key3", 3);
        }

        [Test]
        public void Should_call_callback_when_remove_by_overflow()
        {
            for (var i = 1; i <= 3; i++)
                cache.GetOrAdd($"key{i}", _ => i);

            onRemove.ReceivedCalls().Count().Should().Be(1);
            onRemove.Received(1).Invoke(new KeyValuePair<string, int>("key1", 1));
        }

        private void AssertHasValue(string key, int expectedValue)
        {
            cache.TryGetValue(key, out var value).Should().BeTrue();
            value.Should().Be(expectedValue);
        }
    }
}