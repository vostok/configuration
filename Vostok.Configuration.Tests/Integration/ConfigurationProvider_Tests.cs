using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Extensions;

namespace Vostok.Configuration.Tests.Integration
{
    [TestFixture]
    internal class ConfigurationProvider_Tests : TreeConstructionSet
    {
        private IConfigurationProvider provider;
        private TestConfigurationSource source;

        [SetUp]
        public void TestSetup()
        {
            source = new TestConfigurationSource();
            provider = new ConfigurationProvider().WithSourceFor<string>(source);
        }

        [Test]
        public void ObserveWithErrors_should_run_observer_callbacks_on_dedicated_thread()
        {
            provider.ObserveWithErrors<string>().Subscribe(_ => Thread.Sleep(-1));

            Task.Run(() => source.PushNewConfiguration(Value("xx"))).ShouldCompleteIn(100.Milliseconds());
        }

        [Test]
        public void ObserveWithErrors_source_should_run_observer_callbacks_on_dedicated_thread()
        {
            provider.ObserveWithErrors<int>(source).Subscribe(_ => Thread.Sleep(-1));

            Task.Run(() => source.PushNewConfiguration(Value("42"))).ShouldCompleteIn(100.Milliseconds());
        }

        [Test]
        public void Observe_should_run_observer_callbacks_on_dedicated_thread()
        {
            provider.Observe<string>().Subscribe(_ => Thread.Sleep(-1));

            Task.Run(() => source.PushNewConfiguration(Value("xx"))).ShouldCompleteIn(100.Milliseconds());
        }

        [Test]
        public void Observe_source_should_run_observer_callbacks_on_dedicated_thread()
        {
            provider.Observe<int>(source).Subscribe(_ => Thread.Sleep(-1));

            Task.Run(() => source.PushNewConfiguration(Value("42"))).ShouldCompleteIn(100.Milliseconds());
        }
    }
}