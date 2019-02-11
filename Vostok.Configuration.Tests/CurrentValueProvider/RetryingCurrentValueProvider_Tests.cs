using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Tests.CurrentValueProvider
{
    [TestFixture]
    internal class RetryingCurrentValueProvider_Tests
    {
        private RetryingCurrentValueProvider<object> retryingCurrentValueProvider;
        private ICurrentValueProvider<object>[] providers;
        private Func<ICurrentValueProvider<object>> currentValueProviderFactory;

        [SetUp]
        public void SetUp()
        {
            providers = Enumerable.Range(0, 3).Select(_ => Substitute.For<ICurrentValueProvider<object>>()).ToArray();
            currentValueProviderFactory = Substitute.For<Func<ICurrentValueProvider<object>>>();
            currentValueProviderFactory.Invoke().Returns(providers[0], providers[1], providers[2]);
            retryingCurrentValueProvider = new RetryingCurrentValueProvider<object>(currentValueProviderFactory);
        }

        [Test]
        public void Should_use_currentValueProvider()
        {
            var result = new object();
            providers[0].Get().Returns(result);

            retryingCurrentValueProvider.Get().Should().BeSameAs(result);
        }

        [Test]
        public void Should_recreate_currentValueProvider_when_current_throws()
        {
            providers[0].Get().ThrowsForAnyArgs<Exception>();
            
            var result = new object();
            providers[1].Get().Returns(result);

            retryingCurrentValueProvider.Get().Should().BeSameAs(result);
            
            providers[0].Received(1).Dispose();
        }

        [Test]
        public void Should_throw_when_currentValueProvider_throws_twice()
        {
            providers[0].Get().ThrowsForAnyArgs<Exception>();
            providers[1].Get().ThrowsForAnyArgs<Exception>();
            
            new Action(() => retryingCurrentValueProvider.Get()).Should().Throw<Exception>();
            
            providers[0].Received(1).Dispose();
        }
    }
}