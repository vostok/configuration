using System;
using System.IO;
using System.Reactive.Subjects;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Cache;
using Vostok.Configuration.ObservableBinding;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ObservableBinder_Tests
    {
        private ICachingBinder binder;
        private ObservableBinder observableBinder;
        private ReplaySubject<(ISettingsNode settings, Exception error)> subject;
        private ISettingsNode node;
        private object settings;
        private SourceCacheItem<object> cacheItem;

        [SetUp]
        public void SetUp()
        {
            binder = Substitute.For<ICachingBinder>();
            observableBinder = new ObservableBinder(binder);
            subject = new ReplaySubject<(ISettingsNode settings, Exception error)>();
            
            node = Substitute.For<ISettingsNode>();
            settings = new object();
            cacheItem = new SourceCacheItem<object>();
        }

        [Test]
        public void Should_bind_using_cachingBinder()
        {
            subject.OnNext((node, null));
            
            observableBinder.SelectBound(subject, () => cacheItem).WaitFirstValue(100.Milliseconds());

            binder.Received(1).Bind(node, cacheItem.BindingCacheItem);
        }

        [Test]
        public void Should_push_successfully_bound_settings()
        {
            Bind(node).Returns(settings);
            
            subject.OnNext((node, null));
            
            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(), (settings, null));
        }

        [TestCase(true, false, Description = "source pushes error")]
        [TestCase(false, true, Description = "binding fails")]
        public void Should_push_settings_from_cache_when_error_occurs_and_has_cached_value(bool sourceError, bool bindingError)
        {
            var error = new Exception();
            cacheItem.LastValue = (settings, null);

            if (bindingError)
                Bind(node).Throws(error);
            else
                Bind(node).Returns(settings);
            
            if (sourceError)
                subject.OnNext((null, error));
            else
                subject.OnNext((node, null));
            
            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(), (settings, error));
        }

        [TestCase(true, false, Description = "source pushes error")]
        [TestCase(false, true, Description = "binding fails")]
        public void Should_push_actual_settings_and_no_error_when_error_disappears(bool sourceError, bool bindingError)
        {
            var error = new Exception();
            cacheItem.LastValue = (settings, null);

            if (bindingError)
                Bind(node).Throws(error);
            else
                Bind(node).Returns(settings);
            
            if (sourceError)
                subject.OnNext((null, error));
            else
                subject.OnNext((node, null));

            var node2 = Substitute.For<ISettingsNode>();
            var settings2 = new object();
            Bind(node2).Returns(settings2);
            subject.OnNext((node2, null));
            
            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(),(settings, error), (settings2, null));
        }

        [Test]
        public void Should_not_push_same_settings_successively_when_no_errors()
        {
            Bind(node).Returns(settings);
            
            subject.OnNext((node, null));
            subject.OnNext((node, null));

            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(),(settings, null));
        }
        
        [Test]
        public void Should_push_same_settings_when_they_appear_not_successively()
        {
            var node2 = Substitute.For<ISettingsNode>();
            var settings2 = new object();
            
            Bind(node).Returns(settings);
            Bind(node2).Returns(settings2);
            
            subject.OnNext((node, null));
            subject.OnNext((node2, null));
            subject.OnNext((node, null));

            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(), (settings, null), (settings2, null), (settings, null));
        }

        [TestCase(true, false, Description = "source pushes error")]
        [TestCase(false, true, Description = "binding fails")]
        public void Should_not_push_same_cached_settings_and_error_successively_when_same_errors_occur(bool sourceError, bool bindingError)
        {
            var error = new Exception();
            
            cacheItem.LastValue = (settings, null);

            if (bindingError)
                Bind(node).Throws(error);

            for (var i = 0; i < 2; i++)
            {
                if (sourceError)
                    subject.OnNext((null, error));
                else if (bindingError)
                    subject.OnNext((node, null));
            }

            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(), (settings, error));
        }
        
        [TestCase(true, false, Description = "source pushes error")]
        [TestCase(false, true, Description = "binding fails")]
        public void Should_push_same_cached_settings_and_error_when_same_errors_occur_not_successively(bool sourceError, bool bindingError)
        {
            var errorIndex = 0;
            var errors = new Exception[] {new IOException(), new FormatException(), new IOException()};
            
            cacheItem.LastValue = (settings, null);
            
            if (bindingError)
                Bind(null).ReturnsForAnyArgs(_ => throw errors[errorIndex++]);
            else
                Bind(node).Returns(settings);

            foreach (var error in errors)
                if (sourceError)
                    subject.OnNext((null, error));
                else
                    subject.OnNext((Substitute.For<ISettingsNode>(), null));

            observableBinder.SelectBound(subject, () => cacheItem)
                .ShouldStartWithIn(100.Milliseconds(), (settings, errors[0]), (settings, errors[1]), (settings, errors[2]));
        }

        [Test]
        public void Should_push_same_settings_when_different_calls()
        {
            Bind(node).Returns(settings);
            
            subject.OnNext((node, null));
            
            observableBinder.SelectBound(subject, () => cacheItem).ShouldStartWithIn(100.Milliseconds(), (settings, null));
            observableBinder.SelectBound(subject, () => cacheItem).ShouldStartWithIn(100.Milliseconds(), (settings, null));
        }

        [Test]
        public void Should_call_onError_when_failed_to_bind_settings_and_no_value_in_cache()
        {
            var bindError = new SettingsBindingException("");
            Bind(node).Throws(bindError);
            
            subject.OnNext((node, null));

            observableBinder.SelectBound(subject, () => cacheItem).ShouldCompleteWithError(bindError);
        }
        
        [Test]
        public void Should_call_onError_when_source_pushes_error_and_no_value_in_cache()
        {
            var error = new IOException();
            subject.OnNext((null, error));
            
            observableBinder.SelectBound(subject, () => cacheItem).ShouldCompleteWithError(error);
        }

        [Test]
        public void Should_call_onError_when_source_observable_completes_with_error()
        {
            var error = new IOException();
            subject.OnError(error);
            
            observableBinder.SelectBound(subject, () => cacheItem).ShouldCompleteWithError(error);
        }

        private object Bind(ISettingsNode node)
        {
            return binder.Bind(node, Arg.Any<CachingBinder.BindingCacheItem<object>>());
        }
    }
}