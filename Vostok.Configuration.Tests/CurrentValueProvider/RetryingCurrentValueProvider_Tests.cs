using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
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
            SetupProvidersFactory();

            retryingCurrentValueProvider = new RetryingCurrentValueProvider<object>(currentValueProviderFactory, TimeSpan.Zero);
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

        [Test]
        public void Should_incur_a_cooldown_after_recreating_current_value_provider()
        {
            SetupProvidersFactory();

            retryingCurrentValueProvider = new RetryingCurrentValueProvider<object>(currentValueProviderFactory, 250.Milliseconds());

            var result = new object();

            providers[0].Get().ThrowsForAnyArgs<Exception>();
            providers[1].Get().ThrowsForAnyArgs<Exception>();
            providers[2].Get().Returns(result);

            var getAction = new Action(() => retryingCurrentValueProvider.Get());

            getAction.Should().Throw<Exception>();

            new Action(() => getAction.Should().NotThrow()).ShouldPassIn(10.Seconds());

            retryingCurrentValueProvider.Get().Should().BeSameAs(result);

            providers[0].Received(1).Dispose();
            providers[1].Received(1).Dispose();
            providers[2].DidNotReceive().Dispose();
        }

        private void SetupProvidersFactory()
        {
            providers = Enumerable.Range(0, 3).Select(_ => Substitute.For<ICurrentValueProvider<object>>()).ToArray();
            currentValueProviderFactory = Substitute.For<Func<ICurrentValueProvider<object>>>();
            currentValueProviderFactory.Invoke().Returns(providers[0], providers[1], providers[2]);
        }
    }
}