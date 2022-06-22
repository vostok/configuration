using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Tests.Integration
{
    [TestFixture]
    internal class ConfigurationProvider_Tests : TreeConstructionSet
    {
        private ConfigurationProvider provider;
        private TestConfigurationSource source;

        [SetUp]
        public void TestSetup()
        {
            source = new TestConfigurationSource();
            provider = new ConfigurationProvider(new ConfigurationProviderSettings {SourceRetryCooldown = TimeSpan.Zero});
            provider.SetupSourceFor<string>(source);
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

        [Test]
        public void ObserveWithErrors_should_obtain_new_observable_from_source_upon_receiving_OnError()
        {
            provider.SetupSourceFor<string>(CreateFaultySource());
            provider.ObserveWithErrors<string>().Wait().Should().Be(("3", null));
        }

        [Test]
        public void ObserveWithErrors_source_should_obtain_new_observable_from_source_upon_receiving_OnError()
        {
            provider.ObserveWithErrors<int>(CreateFaultySource()).Wait().Should().Be((3, null));
        }

        [Test]
        public void Observe_should_obtain_new_observable_from_source_upon_receiving_OnError()
        {
            provider.SetupSourceFor<string>(CreateFaultySource());
            provider.Observe<string>().Wait().Should().Be("3");
        }

        [Test]
        public void Observe_source_should_obtain_new_observable_from_source_upon_receiving_OnError()
        {
            provider.Observe<int>(CreateFaultySource()).Wait().Should().Be(3);
        }

        [Test]
        public void Get_with_preconfigured_source_should_invoke_settings_callback()
        {
            source.PushNewConfiguration(new ValueNode("value"));

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            provider = new ConfigurationProvider(new ConfigurationProviderSettings
            {
                SettingsCallback = callback
            });

            provider.SetupSourceFor<string>(source);
            provider.Get<string>();

            callback.Received(1).Invoke("value", source);
        }
        
        [Test]
        public void Get_with_preconfigured_source_should_invoke_error_callback()
        {
            Exception error = null;

            provider = new ConfigurationProvider(new ConfigurationProviderSettings
            {
                ErrorCallback = e =>
                {
                    Console.WriteLine($"ErrorCallback({e.Message})");
                    error = e;
                }
            });

            provider.SetupSourceFor<string>(source);
            Console.WriteLine($"HealthCheckResult: {provider.GetHealthCheckResult()}");

            source.PushNewConfiguration(new ValueNode("value1"));
            AssertionAssertions.ShouldPassIn(() =>
                {
                    provider.Get<string>().Should().Be("value1");
                },
                5.Seconds());
            Console.WriteLine($"HealthCheckResult: {provider.GetHealthCheckResult()}");

            var error1 = new Exception("error1");
            source.PushNewConfiguration(null, error1);
            AssertionAssertions.ShouldPassIn(() =>
            {
                error.Should().Be(error1);
            }, 5.Seconds());
            Console.WriteLine($"HealthCheckResult: {provider.GetHealthCheckResult()}");

            source.PushNewConfiguration(new ValueNode("value2"));
            AssertionAssertions.ShouldPassIn(() =>
            {
                provider.Get<string>().Should().Be("value2");    
            }, 5.Seconds());
            Console.WriteLine($"HealthCheckResult: {provider.GetHealthCheckResult()}");

            var error2 = new Exception("error2");
            source.PushNewConfiguration(null, error2);
            AssertionAssertions.ShouldPassIn(() =>
            {
                error.Should().Be(error2);
            }, 5.Seconds());
            Console.WriteLine($"HealthCheckResult: {provider.GetHealthCheckResult()}");
        }

        [Test]
        public void Get_with_external_source_should_invoke_settings_callback()
        {
            source.PushNewConfiguration(new ValueNode("value"));

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            provider = new ConfigurationProvider(new ConfigurationProviderSettings
            {
                SettingsCallback = callback
            });

            provider.Get<string>(source);

            callback.Received(1).Invoke("value", source);
        }

        [Test]
        public void Observe_with_preconfigured_source_should_invoke_settings_callback()
        {
            source.PushNewConfiguration(new ValueNode("value"));

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            provider = new ConfigurationProvider(new ConfigurationProviderSettings
            {
                SettingsCallback = callback
            });

            provider.SetupSourceFor<string>(source);
            provider.Observe<string>().WaitFirstValue(10.Seconds());

            callback.Received(1).Invoke("value", source);
        }

        [Test]
        public void Observe_with_external_source_should_invoke_settings_callback()
        {
            source.PushNewConfiguration(new ValueNode("value"));

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            provider = new ConfigurationProvider(new ConfigurationProviderSettings
            {
                SettingsCallback = callback
            });

            provider.Observe<string>(source).WaitFirstValue(10.Seconds());

            callback.Received(1).Invoke("value", source);
        }

        private static IConfigurationSource CreateFaultySource()
        {
            var source = Substitute.For<IConfigurationSource>();
            source.Observe()
                .Returns(
                    CreateSource(false, Value("1")),
                    CreateSource(false),
                    CreateSource(true, Value("2"), Value("3")));
            return source;
        }

        private static IObservable<(ISettingsNode, Exception)> CreateSource(bool successful, params ISettingsNode[] nodes)
        {
            var events = nodes.Select(node => Notification.CreateOnNext((node, null as Exception))).ToObservable();
            if (!successful)
                events = events.Append(Notification.CreateOnError<(ISettingsNode, Exception)>(new Exception("oops")));
            return events.Dematerialize();
        }
    }
}