using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Watchers;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class SettingsFileWatcher_Tests
    {
        private const string TestName = nameof(SettingsFileWatcher);
        
        [TearDown]
        public void Cleanup() => TestHelper.DeleteAllFiles(TestName);

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            var watcher = SettingsFileWatcher.WatchFile("file.name");
            var read = false;
            var sub = watcher.Subscribe(s =>
            {
                read = true;
                s.Should().BeNull();
            });
            Thread.Sleep(50.Milliseconds());
            sub.Dispose();
            read.Should().BeTrue();
        }

        [Test]
        public void Should_create_watcher_and_read_file()
        {
            new Action(() => ReturnsIfFileWasRead().Should().BeTrue()).ShouldPassIn(1.Seconds());
        }

        private bool ReturnsIfFileWasRead()
        {
            const string content = "{ \"Param2\": \"set2\" }";
            var fileName = TestHelper.CreateFile(TestName, content);

            var watcher = SettingsFileWatcher.WatchFile(fileName);
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
            const string fileName = "file_name";
            var watcher = SettingsFileWatcher.WatchFile(fileName);
            var anotherWatcher = SettingsFileWatcher.WatchFile(fileName);
            watcher.Should().Be(anotherWatcher);
        }

        [Test, Explicit("Unstable on mass start")]
        public void Should_Observe_file()
        {
            var res = 0;
            new Action(() => res = ReturnsNumberOfSubscribeActionInvokes()).ShouldPassIn(1.Seconds());
            res.Should().Be(4);
        }

        private int ReturnsNumberOfSubscribeActionInvokes()
        {
            const string fileName = TestName + "_ObserveTest.tst";
            const string content = "{ \"Param2\": \"set2\" }";
            var val1 = 0;
            var val2 = 0;
            var sub1 = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(fileName)).Subscribe(
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
            TestHelper.CreateFile(TestName, content, fileName);

            var sub2 = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(fileName)).Subscribe(
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
            const string fileName = TestName + "_CallbackTest.tst";
            var val = 0;
            var jfs = new JsonFileSource(fileName);

            TestHelper.CreateFile(TestName, "wrong file format", fileName);

            var sub1 = jfs.Observe().Subscribe(settings => {}, e => val++);
            var sub2 = jfs.Observe().Subscribe(settings => {}, e => val++);

            Thread.Sleep(200.Milliseconds());

            sub1.Dispose();
            sub2.Dispose();
            
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
            var fileName = TestHelper.CreateFile(TestName, content);

            var sub = ((SingleFileWatcher)SettingsFileWatcher.WatchFile(fileName)).Subscribe(
                s =>
                {
                    val++;
                    s = s?.Trim();
                    s.Should().Be(content);
                });

            Thread.Sleep(200.Milliseconds());

            TestHelper.CreateFile(TestName, "{ \"Param1\": \"set1\" }", fileName);
            Thread.Sleep(200.Milliseconds());

            sub.Dispose();

            return val;
        }
    }
}