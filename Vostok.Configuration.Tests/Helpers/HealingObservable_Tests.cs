using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class HealingObservable_Tests
    {
        [Test]
        public void PushErrors_should_complete_when_source_observable_completes_without_error()
        {
            var subject = new ReplaySubject<(object, Exception)>();

            subject.OnNext((null, new Exception()));
            subject.OnNext((new object(), null));
            subject.OnCompleted();

            HealingObservable.PushErrors(() => subject, 100.Milliseconds())
                .Count().Wait().Should().Be(2);
        }
        
        [Test]
        public void PushErrors_should_push_error_and_restart_source_observable_on_error()
        {
            var errors = Enumerable.Range(0, 3).Select(_ => new Exception()).ToArray();
            var observables = new[]
            {
                CreateObservable(errors[0], CreateValue(1), CreateValue(2)),
                CreateObservable<(int, Exception)>(errors[1]),
                CreateObservable(errors[2], CreateValue(3), CreateValue(4)),
                CreateObservable(true, CreateValue(5))
            };
            var index = 0;

            HealingObservable.PushErrors(() => observables[index++], 100.Milliseconds())
                .ToEnumerable().Should().Equal(
                    (1, null as Exception),
                    (2, null as Exception),
                    (0, errors[0]),
                    (0, errors[1]),
                    (3, null as Exception),
                    (4, null as Exception),
                    (0, errors[2]),
                    (5, null as Exception));
        }
        
        [Test]
        public void PushErrors_should_push_error_immediately()
        {
            var error = new Exception();
            var observables = new[]
            {
                CreateObservable<(int, Exception)>(error),
                CreateObservable(true, CreateValue(1))
            };
            var index = 0;

            Action assertion = () => HealingObservable
                .PushErrors(() => observables[index++], 5.Seconds())
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((0, error));
            
            assertion.ShouldPassIn(1.Seconds());
        }

        private static (T, Exception) CreateValue<T>(T value)
        {
            return (value, null as Exception);
        }

        private static IObservable<T> CreateObservable<T>(bool successful, params T[] values)
        {
            return CreateObservable(successful ? null : new Exception("oops"), values);
        }

        private static IObservable<T> CreateObservable<T>(Exception error, params T[] values)
        {
            var subject = new ReplaySubject<T>();

            foreach (var value in values)
                subject.OnNext(value);

            if (error == null)
                subject.OnCompleted();
            else
                subject.OnError(error);

            return subject;
        }
    }
}