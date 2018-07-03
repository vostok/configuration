using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class XmlFileSource_Tests : Sources_Test
    {
        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            var xfs = new XmlFileSource("some_file");
            xfs.Get().Should().BeNull();
            xfs.Get().Should().BeNull("should work and return same value");
        }

        [Test]
        public void Should_parse_String_value()
        {
            const string fileName = "test.xml";
            const string content = "<StringValue>string</StringValue>";
            SingleFileWatcherSubstitute watcher = null;

            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                return watcher;
            });
            Task.Run(() =>
            {
                while (watcher == null) Thread.Sleep(20);
                watcher.GetUpdate(content);
            });
            var result = xfs.Get();
            result["StringValue"].Value.Should().Be("string");
        }

        [Test]
        public void Should_Get_file_updates()
        {
            const string fileName = "test.xml";
            var content = "<StringValue>string</StringValue>";
            SingleFileWatcherSubstitute watcher = null;

            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                return watcher;
            });
            //create file
            Task.Run(() =>
            {
                while (watcher == null) Thread.Sleep(20);
                watcher.GetUpdate(content);
            });
            var result = xfs.Get();
            result["StringValue"].Value.Should().Be("string");

            content = "<StringValue>string2</StringValue>";
            //update file
            Task.Run(() =>
            {
                Thread.Sleep(100);
                watcher.GetUpdate(content);
            });
            result["StringValue"].Value.Should().Be("string", "did not get updates yet");
            Thread.Sleep(150.Milliseconds());

            result = xfs.Get();
            result["StringValue"].Value.Should().Be("string2");
        }

        [Test]
        public void Should_Observe_file()
        {
            var result = 0;
            new Action(() => result = ShouldObserveFileTest()).ShouldPassIn(1.Seconds());
            result.Should().Be(2);
        }

        private int ShouldObserveFileTest()
        {
            const string fileName = "test.xml";
            const string content = "<Param2>set2</Param2>";
            SingleFileWatcherSubstitute watcher = null;

            var val = 0;
            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                return watcher;
            });
            var sub1 = xfs.Observe().Subscribe(settings =>
            {
                val++;
                settings["Param2"].Value.Should().Be("set2", "#1 on create file");
            });

            //create file
            watcher.GetUpdate(content);
            
            var sub2 = xfs.Observe().Subscribe(settings =>
            {
                val++;
                settings["Param2"].Value.Should().Be("set2", "#2 on create file");
            });

            Thread.Sleep(100.Milliseconds());

            sub1.Dispose();
            sub2.Dispose();

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
            const string fileName = "test.xml";
            var content = "<Param>set1</Param>";
            SingleFileWatcherSubstitute watcher = null;
            
            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                watcher.GetUpdate(content); //create file
                return watcher;
            });
            var sub = xfs.Observe().Subscribe(settings =>
            {
                val++;
                settings["Param"].Value.Should().Be("set1");
            });

            content = "<Param>set1</Param>";
            //update file
            watcher.GetUpdate(content, true);

            sub.Dispose();
            
            return val;
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            const string fileName = "test.xml";
            const string content = "wrong file format";

            new Action(() =>
            {
                var xfs = new XmlFileSource(fileName, (f, e) =>
                {
                    var watcher = new SingleFileWatcherSubstitute(f, e);
                    watcher.GetUpdate(content); //create file
                    return watcher;
                });
                xfs.Get();
            }).Should().Throw<Exception>();
        }

        [Test]
        public void Should_invoke_OnError_for_observer_on_wrong_Xml_format()
        {
            const string value = "wrong file format";
            var next = 0;
            var error = 0;
            new XmlStringSource(value).Observe().SubscribeTo(node => next++, e => error++);

            next.Should().Be(0);
            error.Should().Be(1);
        }

        [Test]
        public void Should_return_last_read_value_if_exception_was_thrown_on_next_read_and_has_no_observers()
        {
            const string fileName = "test.xml";
            var content = "<key>value</key>";
            SingleFileWatcherSubstitute watcher = null;

            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                watcher.GetUpdate(content); //create file
                return watcher;
            });
            var result = xfs.Get();
            result["Key"].Value.Should().Be("value");

            content = "wrong file format";
            //update file
            watcher.GetUpdate(content);

            result = xfs.Get();
            result["Key"].Value.Should().Be("value");
        }

        [Test]
        public void Should_return_OnError_to_subscriber_in_case_of_exception_and_continue_work_after_resubscription()
        {
            const string fileName = "test.xml";
            var content = "wrong file format";
            SingleFileWatcherSubstitute watcher = null;

            var xfs = new XmlFileSource(fileName, (f, e) =>
            {
                watcher = new SingleFileWatcherSubstitute(f, e);
                watcher.GetUpdate(content); //create file
                return watcher;
            });
            var onNext = 0;
            var onError = 0;

            xfs.Observe().Subscribe(node => onNext++, e => onError++);
            Thread.Sleep(50);

            onNext.Should().Be(0);
            onError.Should().Be(1);



            content = "<value>123</value>";
            //update file
            Task.Run(() =>
            {
                Thread.Sleep(20);
                watcher.GetUpdate(content);
            });
            Thread.Sleep(50);

            onNext.Should().Be(0, "need resubscription for changes");



            xfs.Observe().Subscribe(node => onNext++, e => onError++);
            Thread.Sleep(50);

            onNext.Should().Be(1);
            onError.Should().Be(1);
        }
    }
}