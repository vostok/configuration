using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class FixedPeriodSettingsWatcher_Tests
    {
        [Test]
        public void Should_Observe_env_vars()
        {
            new Action(() => ShouldObserveEnvVarsTest().Should().Be(2)).ShouldPassIn(1.Seconds());
        }

        private int ShouldObserveEnvVarsTest()
        {
            const string testVar = "test_key";
            const string testValue = "test_value_1";

            var val = 0;
            using (var evs = new EnvironmentVariablesSource(100.Milliseconds()))
            {
                FixedPeriodSettingsWatcher.StartFixedPeriodSettingsWatcher(100.Milliseconds(), 100.Milliseconds());

                var sub1 = evs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.ChildrenByKey.Should().Contain(pair => pair.Key == testVar && Equals(pair.Value, new RawSettings(testValue)));
                });

                Environment.SetEnvironmentVariable(testVar, testValue);

                var sub2 = evs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.ChildrenByKey.Should().Contain(pair => pair.Key == testVar && Equals(pair.Value, new RawSettings(testValue)));
                });

                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            FixedPeriodSettingsWatcher.StopAndClear();
            return val;
        }

        [Test]
        public void Should_not_Observe_file_twice()
        {
            new Action(() => ShouldNotObserveFileTwiceTest().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        public int ShouldNotObserveFileTwiceTest()
        {
            const string testVar = "test_key";
            const string testValue = "test_value_2";

            var val = 0;
            using (var evs = new EnvironmentVariablesSource(100.Milliseconds()))
            {
                FixedPeriodSettingsWatcher.StartFixedPeriodSettingsWatcher(100.Milliseconds(), 100.Milliseconds());

                var sub = evs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.ChildrenByKey.Should().Contain(pair => pair.Key == testVar && Equals(pair.Value, new RawSettings(testValue)));
                });

                Environment.SetEnvironmentVariable(testVar, testValue);
                Thread.Sleep(200.Milliseconds());

                Environment.SetEnvironmentVariable(testVar, testValue);
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            FixedPeriodSettingsWatcher.StopAndClear();
            return val;
        }

        /*[Test]
        public void Should_callback_on_exception()
        {
            new Action(() => ShouldCallbackOnExceptionTest().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldCallbackOnExceptionTest()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName, 100.Milliseconds(), e => val++))
            {
                var sub1 = jfs.Observe().Subscribe(settings => {});
                var sub2 = jfs.Observe().Subscribe(settings => {});

                CreateTextFile("wrong file format");
                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            SettingsFileWatcher.StopAndClear();
            return val;
        }*/
    }
}