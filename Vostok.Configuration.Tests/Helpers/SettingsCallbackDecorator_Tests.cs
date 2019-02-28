using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class SettingsCallbackDecorator_Tests
    {
        [Test]
        public void Should_not_fail_when_user_callback_is_null()
        {
            new SettingsCallbackDecorator(null, null)
                .Invoke(new object(), Substitute.For<IConfigurationSource>());
        }

        [Test]
        public void Should_not_fail_when_user_callback_throws_an_error()
        {
            var errorCallback = Substitute.For<Action<Exception>>();

            new SettingsCallbackDecorator((_ , __) => throw new Exception(), errorCallback)
                .Invoke(new object(), Substitute.For<IConfigurationSource>());

            errorCallback.Received(1).Invoke(Arg.Any<Exception>());
        }

        [Test]
        public void Should_invoke_user_callback()
        {
            var settings1 = new object();
            var settings2 = new object();

            var source1 = Substitute.For<IConfigurationSource>();
            var source2 = Substitute.For<IConfigurationSource>();

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            var decorator = new SettingsCallbackDecorator(callback, null);

            decorator.Invoke(settings1, source1);
            decorator.Invoke(settings1, source2);
            decorator.Invoke(settings2, source2);
            decorator.Invoke(settings2, source1);

            callback.Received(1).Invoke(settings1, source1);
            callback.Received(1).Invoke(settings1, source2);
            callback.Received(1).Invoke(settings2, source2);
            callback.Received(1).Invoke(settings2, source1);
        }

        [Test]
        public void Should_filter_out_duplicated_calls()
        {
            var settings1 = new object();
            var settings2 = new object();

            var source1 = Substitute.For<IConfigurationSource>();
            var source2 = Substitute.For<IConfigurationSource>();

            var callback = Substitute.For<Action<object, IConfigurationSource>>();

            var decorator = new SettingsCallbackDecorator(callback, null);

            decorator.Invoke(settings1, source1);
            decorator.Invoke(settings1, source1);
            decorator.Invoke(settings1, source2);
            decorator.Invoke(settings1, source2);
            decorator.Invoke(settings2, source2);
            decorator.Invoke(settings2, source2);
            decorator.Invoke(settings2, source1);
            decorator.Invoke(settings2, source1);

            callback.Received(1).Invoke(settings1, source1);
            callback.Received(1).Invoke(settings1, source2);
            callback.Received(1).Invoke(settings2, source2);
            callback.Received(1).Invoke(settings2, source1);
        }
    }
}