using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Cache;
using Vostok.Configuration.CurrentValueProvider;
using Vostok.Configuration.ObservableBinding;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigurationProvider_Tests
    {
        private ConfigurationProvider provider;
        private IConfigurationSource source;
        private object settings;
        private Action<Exception> errorCallback;
        private Action<object, IConfigurationSource> settingsCallback;
        private IObservableBinder observableBinder;
        private ISourceDataCache sourceDataCache;
        private IObservable<(ISettingsNode, Exception)> sourceObservable;
        private ICurrentValueProvider<object> currentValueProvider;
        private ICurrentValueProviderFactory currentValueProviderFactory;

        [SetUp]
        public void SetUp()
        {
            errorCallback = Substitute.For<Action<Exception>>();
            settingsCallback = Substitute.For<Action<object, IConfigurationSource>>();
            observableBinder = Substitute.For<IObservableBinder>();
            sourceDataCache = Substitute.ForPartsOf<SourceDataCache>(10);
            currentValueProviderFactory = Substitute.For<ICurrentValueProviderFactory>();
            provider = new ConfigurationProvider(errorCallback, settingsCallback, observableBinder, sourceDataCache, currentValueProviderFactory);
            
            settings = new object();
            
            source = Substitute.For<IConfigurationSource>();
            sourceObservable = Substitute.For<IObservable<(ISettingsNode, Exception)>>();
            source.Observe().Returns(sourceObservable);
            
            currentValueProvider = Substitute.For<ICurrentValueProvider<object>>();
            currentValueProvider.Get().Returns(settings);
            currentValueProviderFactory.Create<object>(default, default).ReturnsForAnyArgs(currentValueProvider);
            currentValueProviderFactory.WhenForAnyArgs(f => f.Create<object>(default, default))
                .Do(callInfo => callInfo.ArgAt<Func<IObservable<(object, Exception)>>>(0).Invoke());
            
        }

        [Test]
        public void Get_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.Get<object>()).Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Get_should_use_currentValueProvider([Values] bool customSource)
        {
            if (!customSource)
                provider.SetupSourceFor<object>(source);
            Get<object>(customSource).Should().BeSameAs(settings);
        }

        [Test]
        public void Get_should_cache_currentValueProvider_by_type_and_source([Values] bool customSource)
        {
            if (!customSource)
            {
                provider.SetupSourceFor<object>(source);
                provider.SetupSourceFor<int>(source);
            }
            
            Get<object>(customSource).Should().BeSameAs(settings);
            Get<object>(customSource).Should().BeSameAs(settings);

            Get<int>(customSource);

            currentValueProviderFactory.ReceivedWithAnyArgs(1).Create<object>(default, default);
            currentValueProviderFactory.ReceivedWithAnyArgs(1).Create<int>(default, default);
        }

        [Test]
        public void Get_should_wait_for_value_before_saving_currentValueProvider_to_cache_when_custom_source()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            
            currentValueProvider.When(ts => ts.Get()).Do(_ => taskCompletionSource.Task.GetAwaiter().GetResult());

            var task = Task.Run(() => provider.Get<object>(source));
            task.Wait(100.Milliseconds());
            task.IsCompleted.Should().BeFalse();

            var cacheItem = sourceDataCache.GetLimitedCacheItem<object>(source);
            cacheItem.CurrentValueProvider.Should().BeNull();
            
            taskCompletionSource.SetResult(null);

            task.Wait(1.Seconds());
            task.IsCompleted.Should().BeTrue();
            
            cacheItem.CurrentValueProvider.Should().BeSameAs(currentValueProvider);
        }
        
        [Test]
        public void Get_should_dispose_currentValueProvider_when_custom_source_and_failed_to_save_it_to_cacheItem()
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            
            currentValueProvider.When(ts => ts.Get()).Do(_ => taskCompletionSource.Task.GetAwaiter().GetResult());

            var task = Task.Run(() => provider.Get<object>(source));
            task.Wait(50.Milliseconds());
            task.IsCompleted.Should().BeFalse();

            var currentValueProvider2 = Substitute.For<ICurrentValueProvider<object>>();
            var cacheItem = sourceDataCache.GetLimitedCacheItem<object>(source);
            cacheItem.TrySetCurrentValueProvider(currentValueProvider2).Should().BeTrue();
            
            taskCompletionSource.SetResult(null);

            task.Wait(1.Seconds());
            task.IsCompleted.Should().BeTrue();
            
            currentValueProvider.Received(1).Dispose();
        }
        
        [Test]
        public void Get_should_use_persistent_cache_when_preconfigured_source()
        {
            provider.SetupSourceFor<object>(source);
            provider.Get<object>();

            AssertPersistentCacheUsed();
        }

        [Test]
        public void Get_should_use_persistent_cache_when_custom_source_is_already_preconfigured_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.Get<object>(source);

            AssertPersistentCacheUsed();
        }

        [Test]
        public void Get_should_use_limited_cache_when_custom_source_is_not_preconfigured_for_type()
        {
            provider.Get<object>(source);

            AssertLimitedCacheUsed();
        }

        [Test]
        public void Observe_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.Observe<object>()).Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Observe_should_send_error_to_callback_when_hasError([Values] bool hasError, [Values] bool customSource)
        {
            if (!customSource)
                provider.SetupSourceFor<object>(source);
            
            var subject = new Subject<(object, Exception)>();
            observableBinder.SelectBound<object>(default, default).ReturnsForAnyArgs(subject);

            var testObserver = new TestObserver<object>();
            using (Observe<object>(customSource).Subscribe(testObserver))
            {
                var settings = new object();
                var error = hasError ? new Exception() : null;
                
                subject.OnNext((settings, null));
                subject.OnNext((settings, error));

                new Action(() => testObserver.Values.Should().Equal(settings))
                    .ShouldPassIn(5.Seconds());

                if (hasError)
                    errorCallback.Received(1).Invoke(error);
                else
                    errorCallback.DidNotReceiveWithAnyArgs().Invoke(null);

                testObserver.Messages.Should().NotContain(n => n.Kind == NotificationKind.OnError);
            }
        }
        
        [Test]
        public void Observe_should_ignore_error_when_no_callback([Values] bool hasError, [Values] bool customSource)
        {
            provider = new ConfigurationProvider(null, null, observableBinder, sourceDataCache, currentValueProviderFactory);
            if (!customSource)
                provider.SetupSourceFor<object>(source);
            
            var subject = new Subject<(object, Exception)>();
            observableBinder.SelectBound<object>(default, default).ReturnsForAnyArgs(subject);

            var testObserver = new TestObserver<object>();
            using (Observe<object>(customSource).Subscribe(testObserver))
            {
                var settings = new object();
                var error = hasError ? new Exception() : null;
                
                subject.OnNext((settings, null));
                subject.OnNext((settings, error));

                new Action(() => testObserver.Values.Should().Equal(settings))
                    .ShouldPassIn(5.Seconds());

                testObserver.Messages.Should().NotContain(n => n.Kind == NotificationKind.OnError);
            }
        }

        [Test]
        public void Observe_should_not_return_duplicate_settings([Values] bool customSource)
        {
            provider = new ConfigurationProvider(null, null, observableBinder, sourceDataCache, currentValueProviderFactory);
            if (!customSource)
                provider.SetupSourceFor<object>(source);

            var subject = new Subject<(object, Exception)>();
            observableBinder.SelectBound<object>(default, default).ReturnsForAnyArgs(subject);

            var testObserver = new TestObserver<object>();
            using (Observe<object>(customSource).Subscribe(testObserver))
            {
                var settings1 = new object();
                var settings2 = new object();

                subject.OnNext((settings1, null));
                subject.OnNext((settings1, new Exception("1")));
                subject.OnNext((settings1, new Exception("2")));
                subject.OnNext((settings1, null));
                subject.OnNext((settings2, null));

                new Action(() => testObserver.Values.Should().Equal(settings1, settings2))
                    .ShouldPassIn(5.Seconds());
            }
        }

        [Test]
        public void ObserveWithErrors_should_throw_when_no_source_preconfigured_for_type()
        {
            new Action(() => provider.ObserveWithErrors<object>()).Should().Throw<ArgumentException>();
        }

        [Test]
        public void ObserveWithErrors_should_use_persistent_cache_for_preconfigured_source()
        {
            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>();
            
            AssertPersistentCacheUsed();
        }

        [Test]
        public void ObserveWithErrors_should_use_persistent_cache_when_custom_source_is_already_preconfigured_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>(source);
            
            AssertPersistentCacheUsed();
        }
        
        [Test]
        public void ObserveWithErrors_should_use_limited_cache_when_custom_source_is_not_preconfigured_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>(source);
            
            AssertLimitedCacheUsed();
        }

        [Test]
        public void ObserveWithErrors_should_use_observableBinder([Values] bool customSource)
        {
            if (!customSource)
                provider.SetupSourceFor<object>(source);
            
            var result = Substitute.For<IObservable<(object, Exception)>>();
            observableBinder.SelectBound(Arg.Any<IObservable<(ISettingsNode, Exception)>>(), Arg.Any<Func<SourceCacheItem<object>>>()).Returns(result);

            ExtractSource(ObserveWithErrors<object>(customSource)).Should().Be(result);
        }

        [Test]
        public void SetupSourceFor_should_throw_when_Get_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.Get<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_throw_when_Observe_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.Observe<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_throw_when_ObserveWithErrors_was_called_for_type()
        {
            provider.SetupSourceFor<object>(source);
            provider.ObserveWithErrors<object>();
            new Action(() => provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void SetupSourceFor_should_not_throw_when_Get_or_Observe_methods_was_not_called_at_all()
        {
            provider.SetupSourceFor<object>(source);

            var newSource = Substitute.For<IConfigurationSource>();
            new Action(() => provider.SetupSourceFor<object>(newSource)).Should().NotThrow();
        }

        [Test]
        public void SetupSourceFor_should_not_throw_when_Get_method_was_called_for_another_type()
        {
            provider.SetupSourceFor<string>(source);
            provider.Get<string>(source);

            var newSource = Substitute.For<IConfigurationSource>();
            new Action(() => provider.SetupSourceFor<object>(newSource)).Should().NotThrow();
        }

        [Test]
        public void SetupSourceFor_should_not_throw_when_Observe_method_was_called_for_another_type()
        {
            provider.SetupSourceFor<string>(source);
            provider.Observe<string>(source);

            var newSource = Substitute.For<IConfigurationSource>();
            new Action(() => provider.SetupSourceFor<object>(newSource)).Should().NotThrow();
        }

        [Test]
        public void SetupSourceFor_should_allow_to_reconfigure_source_for_the_type_before_calling_Get_or_Observe()
        {
            var source2 = Substitute.For<IConfigurationSource>();

            provider.SetupSourceFor<object>(source);
            provider.SetupSourceFor<object>(source2);

            provider.Get<object>();
            provider.Observe<object>();
        }

        [Test]
        public void SetupSourceFor_should_succeed_with_same_source_by_ref_even_after_Get_had_been_called()
        {
            provider.SetupSourceFor<object>(source);

            provider.Get<object>();

            provider.SetupSourceFor<object>(source);
        }

        [Test]
        public void HasSourceFor_should_return_false_when_there_is_no_source_set_up()
        {
            provider.HasSourceFor<object>().Should().BeFalse();
        }

        [Test]
        public void HasSourceFor_should_return_true_when_there_is_a_source_set_up()
        {
            provider.SetupSourceFor<object>(source);

            provider.HasSourceFor<object>().Should().BeTrue();

            provider.Get<object>();

            provider.HasSourceFor<object>().Should().BeTrue();
        }

        [Test]
        public void TrySetupSourceFor_should_succeed_when_there_is_no_source_yet()
        {
            provider.TrySetupSourceFor<object>(source).Should().BeTrue();
        }

        [Test]
        public void TrySetupSourceFor_should_succeed_when_the_same_source_is_already_configured()
        {
            provider.TrySetupSourceFor<object>(source).Should().BeTrue();
            provider.TrySetupSourceFor<object>(source).Should().BeTrue();
            provider.TrySetupSourceFor<object>(source).Should().BeTrue();
        }

        [Test]
        public void TrySetupSourceFor_should_fail_when_another_source_is_already_configured()
        {
            provider.SetupSourceFor<object>(Substitute.For<IConfigurationSource>());

            provider.TrySetupSourceFor<object>(source).Should().BeFalse();
        }

        private T Get<T>(bool customSource)
        {
            return customSource
                ? provider.Get<T>(source)
                : provider.Get<T>();
        }

        private IObservable<T> Observe<T>(bool customSource)
        {
            return customSource
                ? provider.Observe<T>(source)
                : provider.Observe<T>();
        }
        
        private IObservable<(T, Exception)> ObserveWithErrors<T>(bool customSource)
        {
            return customSource
                ? provider.ObserveWithErrors<T>(source)
                : provider.ObserveWithErrors<T>();
        }

        private void AssertPersistentCacheUsed()
        {
            sourceDataCache.ReceivedWithAnyArgs(1).GetPersistentCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetLimitedCacheItem<object>(default);
        }

        private void AssertLimitedCacheUsed()
        {
            sourceDataCache.ReceivedWithAnyArgs(1).GetLimitedCacheItem<object>(default);
            sourceDataCache.DidNotReceiveWithAnyArgs().GetPersistentCacheItem<object>(default);
        }

        private IObservable<T> ExtractSource<T>(IObservable<T> wrappedObservable)
        {
            return wrappedObservable.GetType().GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(wrappedObservable) as IObservable <T> ?? wrappedObservable;
        }
    }
}