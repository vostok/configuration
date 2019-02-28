using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Tests.CurrentValueProvider
{
    [TestFixture]
    internal class RawCurrentValueProvider_Tests
    {
        private RawCurrentValueProvider<object> provider;
        private ReplaySubject<(object, Exception)> subject;
        private Action<Exception> errorCallback;
        private object value;

        [SetUp]
        public void SetUp()
        {
            errorCallback = Substitute.For<Action<Exception>>();
            provider = new RawCurrentValueProvider<object>(() => subject, errorCallback);
            subject = new ReplaySubject<(object, Exception)>(1);
            value = new object();
        }

        [Test]
        public void Should_wait_for_the_first_value_from_observable()
        {
            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnNext((value, null));

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Result.Should().BeSameAs(value);
        }

        [Test]
        public void Should_cache_value()
        {
            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnNext((value, null));

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();

            provider.Get().Should().BeSameAs(value);
            provider.Get().Should().BeSameAs(value);
        }

        [Test]
        public void Should_update_value_when_observable_pushes_new_value()
        {
            subject.OnNext((value, null));
            provider.Get();
            
            var newValue = new object();
            
            subject.OnNext((newValue, null));

            provider.Get().Should().Be(newValue);
        }

        [Test]
        public void Should_throw_when_observable_immediately_completes_with_error()
        {
            var error = new IOException();
            
            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnError(error);

            Task.WaitAny(new Task[]{getTask}, 1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Exception.InnerException.Should().BeSameAs(error);

            errorCallback.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_throw_when_disposed_before_the_first_value_received()
        {
            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            provider.Dispose();

            Task.WaitAny(new Task[]{getTask}, 1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Exception.InnerException.Should().BeOfType<ObjectDisposedException>();

            errorCallback.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_throw_when_observable_returns_a_pair_with_error()
        {
            var error = new IOException();

            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();

            subject.OnNext((value, error));

            Task.WaitAny(new Task[] { getTask }, 1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Exception.InnerException.Should().BeSameAs(error);

            errorCallback.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_ignore_errors_after_receiving_first_valid_value_and_pass_them_to_error_callback()
        {
            subject.OnNext((value, null));

            provider.Get();

            var error1 = new Exception("1");
            var error2 = new Exception("2");

            subject.OnNext((null, error2));
            subject.OnError(error1);

            provider.Get().Should().BeSameAs(value);

            errorCallback.Received(1).Invoke(error1);
            errorCallback.Received(1).Invoke(error2);
        }
    }
}