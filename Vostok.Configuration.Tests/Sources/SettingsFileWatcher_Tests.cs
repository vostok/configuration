using System;
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
    [SingleThreaded]
    public class SettingsFileWatcher_Tests
    {
        private const string TestFileName1 = "test_SettingsFileWatcher_1.json";
        private const string TestFileName2 = "test_SettingsFileWatcher_2.json";
        private const string TestFileName3 = "test_SettingsFileWatcher_3.json";
        private const string TestFileName4 = "test_SettingsFileWatcher_4.json";

        [SetUp]
        public void SetUp()
        {
            Cleanup();
        }
        
        [TearDown]
        public void Cleanup()
        {
            DeleteTextFiles();
        }

        private static void CreateTextFile(string text, int n = 1)
        {
            var name = string.Empty;
            switch (n)
            {
                case 1: name = TestFileName1;   break;
                case 2: name = TestFileName2;   break;
                case 3: name = TestFileName3;   break;
                case 4: name = TestFileName4;   break;
            }
            using (var file = new StreamWriter(name, false))
                file.WriteLine(text);
        }

        private static void DeleteTextFiles()
        {
            File.Delete(TestFileName1);
            File.Delete(TestFileName2);
            File.Delete(TestFileName3);
            File.Delete(TestFileName4);
        }

        [Test]
        [Order(1)]
        public void Should_create_watcher_and_read_file()
        {
            new Action(() => ReturnsIfFileWasRead().Should().BeTrue()).ShouldPassIn(1.Seconds());
        }

        private bool ReturnsIfFileWasRead()
        {
            const string content = "{ \"Param2\": \"set2\" }";
            CreateTextFile(content);

            var watcher = SettingsFileWatcher.WatchFile(TestFileName1);
            var read = false;
            var sub = watcher.Subscribe(
                s =>
                {
                    read = true;
                    s.Should().Be(content);
                });

            Thread.Sleep(200.Milliseconds());
            sub.Dispose();

            return read;
        }

        [Test]
        public void Should_return_watcher_from_cache()
        {
            var watcher = SettingsFileWatcher.WatchFile(TestFileName1);
            var anotherWatcher = SettingsFileWatcher.WatchFile(TestFileName1);
            watcher.Should().Be(anotherWatcher);
        }

        [Test]
        public void Should_Observe_file()
        {
            var res = 0;
            new Action(() => res = ReturnsNumberOfSubscribeActionInvokes()).ShouldPassIn(1.Seconds());
            res.Should().Be(4);
        }

        private int ReturnsNumberOfSubscribeActionInvokes()
        {
            const string content = "{ \"Param2\": \"set2\" }";
            var val1 = 0;
            var val2 = 0;
            var sub1 = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(TestFileName2)).Subscribe(
                s =>
                {
                    s = s?.Trim();
                    val1++;
                    if (val1 == 1)
                        s.Should().BeNull();
                    else
                        s.Should().Be(content);
                });

            Thread.Sleep(200.Milliseconds());
            CreateTextFile(content, 2);

            var sub2 = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(TestFileName2)).Subscribe(
                s =>
                {
                    s = s?.Trim();
                    val2++;
                    if (val2 == 1)
                        s.Should().BeNull();
                    else
                        s.Should().Be(content);
                });

            Thread.Sleep(200.Milliseconds());

            sub1.Dispose();
            sub2.Dispose();

            return val1 + val2;
        }

        [Test]
        public void Should_callback_on_exception()
        {
            var res = 0;
            new Action(() => res = ReturnsNumberOfCallbacks()).ShouldPassIn(7.Seconds());
            res.Should().Be(2);
        }

        private int ReturnsNumberOfCallbacks()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName3))
            {
                CreateTextFile("wrong file format", 3);

                var sub1 = jfs.Observe().Subscribe(settings => {}, e => val++);
                var sub2 = jfs.Observe().Subscribe(settings => {}, e => val++);

                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            return val;
        }

        [Test]
        public void Should_not_Observe_file_twice()
        {
            new Action(() => ReturnsNumberOfSubscribeActionInvokes_2().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ReturnsNumberOfSubscribeActionInvokes_2()
        {
            const string content = "{ \"Param1\": \"set1\" }";
            var val = 0;
            CreateTextFile(content, 4);

            var sub = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(TestFileName4)).Subscribe(
                s =>
                {
                    val++;
                    s = s?.Trim();
                    s.Should().Be(content);
                });

            Thread.Sleep(200.Milliseconds());

            CreateTextFile("{ \"Param1\": \"set1\" }", 4);
            Thread.Sleep(200.Milliseconds());

            sub.Dispose();

            return val;
        }
    }
}