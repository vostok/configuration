using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonFileSource_Tests
    {
        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var jfs = new JsonFileSource("some_file"))
            {
                jfs.Get().Should().BeNull();
                jfs.Get().Should().BeNull("should work and return same value");
            }
        }

        [Test]
        public void Should_parse_String_value()
        {
            const string fileName = "test.json";
            const string content = "{ 'StringValue': 'string' }";
            SingleFileWatcherSubstitute watcher = null;

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                return watcher;
            }))
            {
                Task.Run(() =>
                {
                    while (watcher == null) Thread.Sleep(20);
                    watcher.GetUpdate(content);
                });
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");
            }
        }

        [Test]
        public void Should_Get_file_updates()
        {
            const string fileName = "test.json";
            var content = "{ 'StringValue': 'string' }";
            SingleFileWatcherSubstitute watcher = null;

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                return watcher;
            }))
            {
                //create file
                Task.Run(() =>
                {
                    while (watcher == null) Thread.Sleep(20);
                    watcher.GetUpdate(content);
                });
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");

                content = "{ 'StringValue': 'string2' }";
                //update file
                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    watcher.GetUpdate(content);
                });
                result["StringValue"].Value.Should().Be("string", "did not get updates yet");
                Thread.Sleep(150.Milliseconds());

                result = jfs.Get();
                result["StringValue"].Value.Should().Be("string2");
            }
        }

        //todo: fails sometimes
        [Test]
        public void Should_Observe_file()
        {
            var result = 0;
            new Action(() => result = ShouldObserveFileTest()).ShouldPassIn(1.Seconds());
            result.Should().Be(2);
        }

        private int ShouldObserveFileTest()
        {
            const string fileName = "test.json";
            const string content = "{ 'Param2': 'set2' }";
            SingleFileWatcherSubstitute watcher = null;

            var val = 0;
            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                return watcher;
            }))
            {
                var sub1 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param2"].Value.Should().Be("set2", "#1 on create file");
                });

                //create file
                watcher.GetUpdate(content);
                
                var sub2 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param2"].Value.Should().Be("set2", "#2 on create file");
                });

                Thread.Sleep(100.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            return val;
        }

        [Test]
        public void Should_not_Observe_file_twice()
        {
            var res = 0;
            new Action(() => res = ShouldObserveFileTwiceTest_ReturnsCountOfReceives()).ShouldPassIn(1.Seconds());
            res.Should().Be(2);
        }

        private int ShouldObserveFileTwiceTest_ReturnsCountOfReceives()
        {
            var val = 0;
            const string fileName = "test.json";
            var content = "{ 'Param': 'set1' }";
            SingleFileWatcherSubstitute watcher = null;
            
            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
            {
                var sub = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param"].Value.Should().Be("set1");
                });

                content = "{ 'Param': 'set1' }";
                //update file
                watcher.GetUpdate(content, true);

                sub.Dispose();
            }
            return val;
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            const string fileName = "test.json";
            const string content = "wrong file format";

            new Action(() =>
            {
                using (var jfs = new JsonFileSource(fileName, f =>
                {
                    var watcher = new SingleFileWatcherSubstitute(f);
                    watcher.GetUpdate(content); //create file
                    return watcher;
                }))
                    jfs.Get();
            }).Should().Throw<Exception>();
        }

        [Test]
        public void Should_return_last_read_value_if_exception_was_thrown_on_next_read_and_has_no_observers()
        {
            const string fileName = "test.json";
            var content = "{ 'Key': 'value' }";
            SingleFileWatcherSubstitute watcher = null;

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
            {
                var result = jfs.Get();
                result["Key"].Value.Should().Be("value");

                content = "wrong file format";
                //update file
                watcher.GetUpdate(content);

                result = jfs.Get();
                result["Key"].Value.Should().Be("value");
            }
        }
    }
}