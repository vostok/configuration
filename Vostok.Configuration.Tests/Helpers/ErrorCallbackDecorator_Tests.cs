using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class ErrorCallbackDecorator_Tests
    {
        [Test]
        public void Should_not_fail_when_user_callback_is_null()
        {
            new ErrorCallbackDecorator(null).Invoke(new Exception());
        }

        [Test]
        public void Should_not_fail_when_user_callback_throws_an_error()
        {
            new ErrorCallbackDecorator(_ => throw new Exception()).Invoke(new Exception());
        }

        [Test]
        public void Should_invoke_user_callback()
        {
            var callback = Substitute.For<Action<Exception>>();

            var decorator = new ErrorCallbackDecorator(callback);

            var error1 = new Exception("1");
            var error2 = new Exception("2");

            decorator.Invoke(error1);
            decorator.Invoke(error2);

            callback.Received(1).Invoke(error1);
            callback.Received(1).Invoke(error2);
        }

        [Test]
        public void Should_filter_out_duplicate_calls()
        {
            var callback = Substitute.For<Action<Exception>>();

            var decorator = new ErrorCallbackDecorator(callback);

            var error1 = new Exception("1");
            var error2 = new Exception("2");

            decorator.Invoke(error1);
            decorator.Invoke(error1);
            decorator.Invoke(error2);
            decorator.Invoke(error2);

            callback.Received(1).Invoke(error1);
            callback.Received(1).Invoke(error2);
        }
    }
}