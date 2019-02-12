using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class HealingObservable_Tests
    {
        [Test]
        public void Should_complete_when_source_observable_completes_without_error()
        {
            var subject = new ReplaySubject<object>();

            subject.OnNext(new object());
            subject.OnNext(new object());
            subject.OnCompleted();

            HealingObservable.Create(() => subject, 100.Milliseconds())
                .Count().Wait().Should().Be(2);
        }
        
        [Test]
        public void Should_restart_source_observable_on_error()
        {
            var observables = new[]
            {
                CreateObservable(false, 1, 2, 3),
                CreateObservable(false),
                CreateObservable(false, 4, 5),
                CreateObservable(true, 6)
            };
            var index = 0;

            HealingObservable.Create(() => observables[index++], 100.Milliseconds())
                .ToEnumerable().Should().Equal(1, 2, 3, 4, 5, 6);
        }

        private static IObservable<int> CreateObservable(bool successful, params int[] values)
        {
            var subject = new ReplaySubject<int>();

            foreach (var value in values)
                subject.OnNext(value);

            if (successful)
                subject.OnCompleted();
            else
                subject.OnError(new Exception("oops"));

            return subject;
        }
    }
}