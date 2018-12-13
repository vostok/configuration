using System;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigurationProvider_Tests
    {
        private IConfigurationGetter configurationGetter;
        private IConfigurationObservable configurationObservable;
        private IConfigurationWithErrorsObservable configurationWithErrorsObservable;
        private ConfigurationProvider provider;
        private IConfigurationSource source;
        private object settings;

        [SetUp]
        public void SetUp()
        {
            configurationGetter = Substitute.For<IConfigurationGetter>();
            configurationObservable = Substitute.For<IConfigurationObservable>();
            configurationWithErrorsObservable = Substitute.For<IConfigurationWithErrorsObservable>();

            provider = new ConfigurationProvider(configurationGetter, configurationObservable, configurationWithErrorsObservable);
            source = Substitute.For<IConfigurationSource>();
            settings = new object();
        }

        [Test]
        public void Get_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.Get<object>()).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Get_should_use_configurationGetter_for_preconfigured_source()
        {
            configurationGetter.Get<object>().Returns(settings);
            
            provider.SetupSourceFor<object>(source);
            provider.Get<object>().Should().BeSameAs(settings);
        }

        [Test]
        public void Get_should_use_configurationGetter_for_provided_source()
        {
            configurationGetter.Get<object>(source).Returns(settings);
            
            provider.Get<object>(source).Should().BeSameAs(settings);
        }

        [Test]
        public void Get_with_source_should_use_Get_without_source_when_source_was_preconfigured_for_type()
        {
            configurationGetter.Get<object>().Returns(settings);
            
            provider.SetupSourceFor<object>(source);
            provider.Get<object>(source).Should().BeSameAs(settings);
        }

        [Test]
        public void Observe_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.Observe<object>()).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Observe_should_use_configurationObservable_for_preconfigured_source()
        {
            var observable = Substitute.For<IObservable<object>>();
            configurationObservable.Observe<object>().Returns(observable);

            provider.SetupSourceFor<object>(source);
            provider.Observe<object>().Should().BeSameAs(observable);
        }

        [Test]
        public void Observe_should_use_configurationObservable_for_provided_source()
        {
            var observable = Substitute.For<IObservable<object>>();
            configurationObservable.Observe<object>(source).Returns(observable);

            provider.Observe<object>(source).Should().BeSameAs(observable);
        }
        
        [Test]
        public void Observe_with_source_should_use_Observe_without_source_when_source_was_preconfigured_for_type()
        {
            var observable = Substitute.For<IObservable<object>>();
            configurationObservable.Observe<object>().Returns(observable);

            provider.SetupSourceFor<object>(source);
            provider.Observe<object>(source).Should().BeSameAs(observable);
        }

        [Test]
        public void ObserveWithErrors_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.ObserveWithErrors<object>()).Should().Throw<ArgumentException>();
        }

        [Test]
        public void ObserveWithErrors_should_use_configurationWithErrorsObservable_for_preconfigured_source()
        {
            var observable = Substitute.For<IObservable<(object, Exception)>>();
            configurationWithErrorsObservable.ObserveWithErrors<object>().Returns(observable);

            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>().Should().BeSameAs(observable);
        }

        [Test]
        public void ObserveWithErrors_should_use_configurationWithErrorsObservable_for_provided_source()
        {
            var observable = Substitute.For<IObservable<(object, Exception)>>();
            configurationWithErrorsObservable.ObserveWithErrors<object>(source).Returns(observable);

            provider.ObserveWithErrors<object>(source).Should().BeSameAs(observable);
        }

        [Test]
        public void ObserveWithErrors_with_source_should_use_ObserveWithErrors_without_source_when_source_was_preconfigured_for_type()
        {
            var observable = Substitute.For<IObservable<(object, Exception)>>();
            configurationWithErrorsObservable.ObserveWithErrors<object>().Returns(observable);

            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>(source).Should().BeSameAs(observable);
        }

        [Test]
        public void SetupSourceFor_should_throw_when_Get_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.Get<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_throw_when_Observe_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.Observe<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_throw_when_ObserveWithErrors_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_not_throw_when_Get_or_Observe_methods_was_not_called()
        {
            provider.SetupSourceFor<object>(source);
            
            var newSource = Substitute.For<IConfigurationSource>();
            new Action(() => provider.SetupSourceFor<object>(newSource)).Should().NotThrow();
        }
    }
}