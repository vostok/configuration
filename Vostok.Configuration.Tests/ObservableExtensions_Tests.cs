using System;
using System.Reactive.Subjects;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Helpers;
using Vostok.Commons.Testing.Observable;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    internal class ObservableExtensions_Tests
    {
        [Test]
        public void SendErrorsToCallback_should_work_correctly([Values] bool hasError)
        {
            var errorCallback = Substitute.For<Action<Exception>>();
            var subject = new Subject<(object, Exception)>();

            var testObserver = new TestObserver<object>();
            using (subject.SendErrorsToCallback(errorCallback).Subscribe(testObserver))
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
    }
}