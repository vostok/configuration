using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Configuration.TaskSource;

namespace Vostok.Configuration.Tests.TaskSource
{
    [TestFixture]
    internal class TaskSource_Tests
    {
        private TaskSource<object> taskSource;
        private ICurrentValueObserver<object>[] observers;
        private Func<ICurrentValueObserver<object>> currentValueObserverFactory;
        private Func<IObservable<object>> observableProvider;

        [SetUp]
        public void SetUp()
        {
            observers = Enumerable.Range(0, 3).Select(_ => Substitute.For<ICurrentValueObserver<object>>()).ToArray();
            currentValueObserverFactory = Substitute.For<Func<ICurrentValueObserver<object>>>();
            currentValueObserverFactory.Invoke().Returns(observers[0], observers[1], observers[2]);
            observableProvider = Substitute.For<Func<IObservable<object>>>();
            taskSource = new TaskSource<object>(observableProvider, currentValueObserverFactory);
        }

        [Test]
        public void Should_use_currentValueObserver()
        {
            var result = new object();
            observers[0].Get(observableProvider).Returns(result);

            taskSource.Get().Should().BeSameAs(result);
        }

        [Test]
        public void Should_recreate_currentValueObserver_when_current_throws()
        {
            observers[0].Get(observableProvider).ThrowsForAnyArgs<Exception>();
            
            var result = new object();
            observers[1].Get(observableProvider).Returns(result);

            taskSource.Get().Should().BeSameAs(result);
            
            observers[0].Received(1).Dispose();
        }

        [Test]
        public void Should_throw_when_currentValueObserver_throws_twice()
        {
            observers[0].Get(observableProvider).ThrowsForAnyArgs<Exception>();
            observers[1].Get(observableProvider).ThrowsForAnyArgs<Exception>();
            
            new Action(() => taskSource.Get()).Should().Throw<Exception>();
            
            observers[0].Received(1).Dispose();
        }
    }
}