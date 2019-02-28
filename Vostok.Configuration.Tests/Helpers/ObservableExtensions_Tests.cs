using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using System.Reactive.Subjects;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Tests.Helpers
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


                if (hasError)
                {
                    errorCallback.Received(1).Invoke(error);
                    testObserver.Values.Should().BeEmpty();
                }
                else
                {
                    errorCallback.DidNotReceiveWithAnyArgs().Invoke(null);
                    testObserver.Values.Should().Equal(settings);
                }
            }
        }
    }
}