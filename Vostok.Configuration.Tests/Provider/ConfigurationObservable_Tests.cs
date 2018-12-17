using System;
using System.Reactive.Subjects;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing.ObservableHelpers;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Tests.Provider
{
    internal class ConfigurationObservable_Tests
    {
        private ConfigurationObservable configurationObservable;
        private IConfigurationWithErrorsObservable configurationWithErrorsObservable;
        private Action<Exception> errorCallback;
        private IConfigurationSource source;

        [SetUp]
        public void SetUp()
        {
            configurationWithErrorsObservable = Substitute.For<IConfigurationWithErrorsObservable>();
            errorCallback = Substitute.For<Action<Exception>>();
            configurationObservable = new ConfigurationObservable(configurationWithErrorsObservable, errorCallback);

            source = Substitute.For<IConfigurationSource>();
        }

        [Test]
        public void Should_use_corresponding_method_from_underlying_interface_when_preconfigured_source()
        {
            configurationObservable.Observe<object>();

            configurationWithErrorsObservable.Received(1).ObserveWithErrors<object>();
        }

        [Test]
        public void Should_use_corresponding_method_from_underlying_interface_when_custom_source()
        {
            configurationObservable.Observe<object>(source);

            configurationWithErrorsObservable.Received(1).ObserveWithErrors<object>(source);
        }

        [Test]
        public void Should_send_error_to_callback_when_hasError([Values] bool hasError, [Values] bool customSource)
        {
            var subject = new Subject<(object, Exception)>();
            ObserveWithErrors(customSource).Returns(subject);

            var testObserver = new TestObserver<object>();
            using (Observe(customSource).Subscribe(testObserver))
            {
                var settings = new object();
                var error = hasError ? new Exception() : null;
                
                subject.OnNext((settings, error));

                testObserver.Values.Should().Equal(settings);

                if (hasError)
                    errorCallback.Received(1).Invoke(error);
                else
                    errorCallback.DidNotReceiveWithAnyArgs().Invoke(null);
            }
        }
        
        [Test]
        public void Should_ignore_error_when_no_callback([Values] bool hasError, [Values] bool customSource)
        {
            configurationObservable = new ConfigurationObservable(configurationWithErrorsObservable);
            
            var subject = new Subject<(object, Exception)>();
            ObserveWithErrors(customSource).Returns(subject);

            var testObserver = new TestObserver<object>();
            using (Observe(customSource).Subscribe(testObserver))
            {
                var settings = new object();
                var error = hasError ? new Exception() : null;
                
                subject.OnNext((settings, error));

                testObserver.Values.Should().Equal(settings);
            }
        }

        private IObservable<object> Observe(bool customSource)
        {
            return customSource
                ? configurationObservable.Observe<object>(source)
                : configurationObservable.Observe<object>();
        }
        
        private IObservable<(object, Exception)> ObserveWithErrors(bool customSource)
        {
            return customSource
                ? configurationWithErrorsObservable.ObserveWithErrors<object>(source)
                : configurationWithErrorsObservable.ObserveWithErrors<object>();
        }
    }
}