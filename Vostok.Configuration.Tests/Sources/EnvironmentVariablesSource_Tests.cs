using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class EnvironmentVariablesSource_Tests
    {
        [Test]
        [Order(2)]
        public void Should_return_correct_values()
        {
            using (var evs = new EnvironmentVariablesSource())
            {
                var res = evs.Get();
                res["Path"].Should().NotBeNull();
                res["APPDATA"].Should().NotBeNull();
            }
        }

        //todo: fails sometimes
        [Test]
        [Order(1)]
        public void Should_observe_variables()
        {
            new Action(() => ShouldObserveVariablesTest_ReturnsIfChangeWasReceived().Should().BeTrue()).ShouldPassIn(1.Seconds());
        }
        private bool ShouldObserveVariablesTest_ReturnsIfChangeWasReceived()
        {
            var val = false;
            using (var evs = new EnvironmentVariablesSource(100.Milliseconds()))
            {
                var sub = evs.Observe().Subscribe(settings =>
                {
                    val = true;
                    settings.Children.Should().NotBeNullOrEmpty();
                });
                Thread.Sleep(200.Milliseconds());
                sub.Dispose();
            }
            return val;
        }
    }
}