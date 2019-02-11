using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.CurrentValueProvider;

namespace Vostok.Configuration.Tests.CurrentValueProvider
{
    [TestFixture]
    internal class RawCurrentValueProvider_Tests
    {
        private RawCurrentValueProvider<object> provider;
        private ReplaySubject<object> subject;
        private object value;

        [SetUp]
        public void SetUp()
        {
            provider = new RawCurrentValueProvider<object>(() => subject);
            subject = new ReplaySubject<object>(1);
            value = new object();
        }

        [Test]
        public void Should_wait_for_the_first_value_from_observable()
        {
            var getTask = Task.Run(() => provider.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnNext(value);

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
            
            subject.OnNext(value);

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();

            provider.Get().Should().BeSameAs(value);
            provider.Get().Should().BeSameAs(value);
        }

        [Test]
        public void Should_update_value_when_observable_pushes_new_value()
        {
            subject.OnNext(value);
            provider.Get();
            
            var newValue = new object();
            
            subject.OnNext(newValue);

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
        }
    }
}