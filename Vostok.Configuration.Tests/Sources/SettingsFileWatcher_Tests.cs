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
    public class SettingsFileWatcher_Tests
    {
        private const string TestFileName = "test_SettingsFileWatcher.json";

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFileName);
        }

        private static void CreateTextFile(string text)
        {
            using (var file = new StreamWriter(TestFileName, false))
                file.WriteLine(text);
        }

        /*[Test]
        public void Should_Observe_file()
        {
            new Action(() => ReturnsNumberOfSubscribeActionInvokes_1().Should().Be(2)).ShouldPassIn(1.Seconds());
        }

        private int ReturnsNumberOfSubscribeActionInvokes_1()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName, 100.Milliseconds()))
            {
                var sub1 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new Dictionary<string, RawSettings>
                            {
                                {"Param2", new RawSettings("set2")}
                            }));
                });

                CreateTextFile("{ \"Param2\": \"set2\" }");

                var sub2 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new Dictionary<string, RawSettings>
                            {
                                {"Param2", new RawSettings("set2")}
                            }));
                });

                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            SettingsFileWatcher.StopAndClear();
            return val;
        }*/

        /*[Test]
        public void Should_not_Observe_file_twice()
        {
            new Action(() => ReturnsNumberOfSubscribeActionInvokes_2().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        public int ReturnsNumberOfSubscribeActionInvokes_2()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName, 100.Milliseconds()))
            {
                var sub = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new Dictionary<string, RawSettings>
                            {
                                {"Param1", new RawSettings("set1")}
                            }));
                });

                CreateTextFile("{ \"Param1\": \"set1\" }");
                Thread.Sleep(200.Milliseconds());

                CreateTextFile("{ \"Param1\": \"set1\" }");
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            SettingsFileWatcher.StopAndClear();
            return val;
        }*/

        /*[Test]
        public void Should_callback_on_exception()
        {
            new Action(() => ReturnsNumberOfCallbacks().Should().BeGreaterOrEqualTo(2)).ShouldPassIn(1.Seconds());
        }

        private int ReturnsNumberOfCallbacks()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName, 100.Milliseconds(), e => val++))
            {
                var sub1 = jfs.Observe().Subscribe(settings => {});
                var sub2 = jfs.Observe().Subscribe(settings => {});

                CreateTextFile("wrong file format");
                Thread.Sleep(250.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            SettingsFileWatcher.StopAndClear();
            return val;
        }*/
    }
}