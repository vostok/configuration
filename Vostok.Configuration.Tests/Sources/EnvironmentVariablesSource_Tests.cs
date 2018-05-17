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
                res.ChildrenByKey.Keys.Should().Contain("Path").And.Contain("APPDATA");
            }
        }

        [Test]
        public void Should_observe_variables()
        {
            new Action(() => ShouldObserveVariablesTest_ReturnsIfChangeWasReceived().Should().BeTrue()).ShouldPassIn(1.Seconds());
        }
        private bool ShouldObserveVariablesTest_ReturnsIfChangeWasReceived()
        {
            const string testVar = "test_key";
            const string testValue = "test_value";
            var val = false;
            var read = false;
            using (var evs = new EnvironmentVariablesSource(100.Milliseconds()))
            {
                var sub = evs.Observe().Subscribe(settings =>
                {
                    if (settings.ChildrenByKey.ContainsKey(testVar) && read)
                        val = true;
                    read = true;
                });

                Environment.SetEnvironmentVariable(testVar, testValue);
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }
    }
}