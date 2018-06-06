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
        public void Should_return_correct_values()
        {
            using (var evs = new EnvironmentVariablesSource())
            {
                var res = evs.Get();
                res["pAtH"].Value.Should().NotBeNull();
                res["APPdata"].Value.Should().NotBeNull();
            }
        }

        //todo: fails sometimes
        [Test]
        public void Should_observe_variables()
        {
            new Action(() => ShouldObserveVariablesTest_ReturnsIfValueWasReceived().Should().BeTrue()).ShouldPassIn(1.Seconds());
        }
        private bool ShouldObserveVariablesTest_ReturnsIfValueWasReceived()
        {
            var val = false;
            using (var evs = new EnvironmentVariablesSource())
            {
                var sub = evs.Observe().Subscribe(settings =>
                {
                    val = true;
                    settings["Path"].Value.Should().NotBeNull();
                    settings["APPDATA"].Value.Should().NotBeNull();
                });
                Thread.Sleep(200.Milliseconds());
                sub.Dispose();
            }
            return val;
        }
    }
}