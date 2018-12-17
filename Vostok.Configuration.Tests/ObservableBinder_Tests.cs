using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.ObservableHelpers;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ObservableBinder_Tests
    {
        private ISettingsBinder binder;
        private ObservableBinder observer;
        private ReplaySubject<(ISettingsNode settings, Exception error)> subject;
        private ISettingsNode node;
        private object settings;
        private SourceCacheItem<object> cacheItem;

        [SetUp]
        public void SetUp()
        {
            binder = Substitute.For<ISettingsBinder>();
            var cachingBinder = new CachingBinder(binder);
            observer = new ObservableBinder(cachingBinder);
            subject = new ReplaySubject<(ISettingsNode settings, Exception error)>(1);
            
            node = Substitute.For<ISettingsNode>();
            settings = new object();
            cacheItem = new SourceCacheItem<object>();
        }

        [Test]
        public void Should_push_successfully_bound_settings_and_error_from_source([Values]bool sourceValueHasError)
        {
            var error = sourceValueHasError ? new IOException() : null;
            
            binder.Bind<object>(node).Returns(settings);
            
            subject.OnNext((node, error));
            
            observer.SelectBound(subject, () => cacheItem)
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, error));
        }

        [Test]
        public void Should_push_settings_from_cache_when_failed_to_bind_settings_and_has_cached_value([Values]bool sourceValueHasError)
        {
            var sourceError = sourceValueHasError ? new IOException() : null;
            var bindError = new SettingsBindingException("");
            cacheItem.LastValue = (settings, null);

            binder.Bind<object>(node).Throws(bindError);
            
            subject.OnNext((node, sourceError));

            var expectedError = sourceValueHasError
                ? (Exception)new AggregateException(sourceError, bindError)
                : bindError;
            
            observer.SelectBound(subject, () => cacheItem)
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .BeEquivalentTo((settings, expectedError));
        }

        [Test]
        public void Should_not_push_anything_when_failed_to_bind_settings_and_cached_error_is_the_same([Values]bool sourceValueHasError)
        {
            var sourceError = sourceValueHasError ? new IOException() : null;
            
            var bindError = new SettingsBindingException("");
            var cachedBindError = new SettingsBindingException("");

            var cachedError = sourceValueHasError
                ? (Exception)new AggregateException(new IOException(), cachedBindError)
                : cachedBindError; 
            
            cacheItem.LastValue = (settings, cachedError);

            binder.Bind<object>(node).Throws(bindError);
            
            subject.OnNext((node, sourceError));

            observer.SelectBound(subject, () => cacheItem)
                .Buffer(500.Milliseconds(), 1)
                .ToEnumerable()
                .First()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Should_call_onError_when_failed_to_bind_settings_and_no_value_in_cache([Values]bool sourceValueHasError)
        {
            var sourceError = sourceValueHasError ? new IOException() : null;
            var bindError = new SettingsBindingException("");

            binder.Bind<object>(node).Throws(bindError);
            
            subject.OnNext((node, sourceError));

            var expectedError = sourceValueHasError
                ? (Exception)new AggregateException(sourceError, bindError)
                : bindError;

            var testObserver = new TestObserver<(object, Exception)>();
            using (observer.SelectBound(subject, () => cacheItem).Subscribe(testObserver))
            {
                Action assertion = () =>
                {
                    testObserver.Messages.Count.Should().Be(1);
                    testObserver.Messages[0].Kind.Should().Be(NotificationKind.OnError);
                    ExceptionsComparer.Equals(testObserver.Messages[0].Exception, expectedError).Should().BeTrue();
                };
                assertion.ShouldPassIn(100.Milliseconds());
            }
        }

        [Test]
        public void Should_call_onError_when_source_observable_completes_with_error()
        {
            var error = new IOException();
            subject.OnError(error);
            
            var testObserver = new TestObserver<(object, Exception)>();
            using (observer.SelectBound(subject, () => cacheItem).Subscribe(testObserver))
            {
                Action assertion = () =>
                {
                    testObserver.Messages.Count.Should().Be(1);
                    testObserver.Messages[0].Kind.Should().Be(NotificationKind.OnError);
                    ExceptionsComparer.Equals(testObserver.Messages[0].Exception, error).Should().BeTrue();
                };
                assertion.ShouldPassIn(100.Milliseconds());
            }
        }
    }
}