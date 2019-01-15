using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Tests.TaskSource
{
    [TestFixture]
    internal class CurrentValueObserver_Tests
    {
        private CurrentValueObserver<object> observer;
        private ReplaySubject<object> subject;
        private object value;

        [SetUp]
        public void SetUp()
        {
            observer = new CurrentValueObserver<object>();
            subject = new ReplaySubject<object>(1);
            value = new object();
        }

        [Test]
        public void Should_wait_for_the_first_value_from_observable()
        {
            var getTask = Task.Run(() => observer.Get(() => subject));
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
            var subject = new Subject<object>();
            var getTask = Task.Run(() => observer.Get(() => subject));
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnNext(value);

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();

            observer.Get(() => subject).Should().BeSameAs(value);
            observer.Get(() => subject).Should().BeSameAs(value);
        }

        [Test]
        public void Should_update_value_when_observable_pushes_new_value()
        {
            subject.OnNext(value);
            observer.Get(() => subject);
            
            var newValue = new object();
            
            subject.OnNext(newValue);

            observer.Get(() => subject).Should().Be(newValue);
        }

        [Test]
        public void Should_throw_when_observable_immediately_completes_with_error()
        {
            var error = new IOException();
            
            var getTask = Task.Run(() => observer.Get(() => subject));
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            subject.OnError(error);

            Task.WaitAny(new Task[]{getTask}, 1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Exception.InnerException.Should().BeSameAs(error);
        }
    }
}