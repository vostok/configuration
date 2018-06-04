using System;
using System.Threading;
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
        private const string TestName = nameof(JsonFileSource);
        
        [TearDown]
        public void Cleanup()
        {
            TestHelper.DeleteAllFiles(TestName);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var jfs = new JsonFileSource("some_file"))
                jfs.Get().Should().BeNull();
        }

        [Test]
        public void Should_parse_String_value()
        {
            var fileName = TestHelper.CreateFile(TestName, "{ 'StringValue': 'string' }");

            using (var jfs = new JsonFileSource(fileName))
            {
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");
            }
        }

        [Test]
        public void Should_Get_file_updates()
        {
            var fileName = TestHelper.CreateFile(TestName, "{ 'StringValue': 'string' }");

            using (var jfs = new JsonFileSource(fileName))
            {
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");

                TestHelper.CreateFile(TestName, "{ 'StringValue': 'string2' }", fileName);
                result["StringValue"].Value.Should().Be("string", "did not get updates yet");
                Thread.Sleep(300.Milliseconds());

                result = jfs.Get();
                result["StringValue"].Value.Should().Be("string2");
            }
        }

        [Test, Explicit("Not stable on mass tests")]
        public void Should_Observe_file()
        {
            new Action(() => ShouldObserveFileTest().Should().Be(2)).ShouldPassIn(1.Seconds());
        }

        private int ShouldObserveFileTest()
        {
            const string fileName = TestName + "_ObserveTest.tst";

            var val = 0;
            using (var jfs = new JsonFileSource(fileName))
            {
                var sub1 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param2"].Value.Should().Be("set2");
                });

                TestHelper.CreateFile(TestName, "{ 'Param2': 'set2' }", fileName);

                var sub2 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param2"].Value.Should().Be("set2");
                });

                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            return val;
        }

        [Test, Explicit("Not stable on mass tests")]
        public void Should_not_Observe_file_twice()
        {
            new Action(() => ShouldNotObserveFileTwiceTest_ReturnsCountOfReceives().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldNotObserveFileTwiceTest_ReturnsCountOfReceives()
        {
            var val = 0;
            var fileName = TestHelper.CreateFile(TestName, "{ 'Param1': 'set1' }");

            using (var jfs = new JsonFileSource(fileName))
            {
                var sub = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param"].Value.Should().Be("set1");
                });

                TestHelper.CreateFile(TestName, "{ 'Param1': 'set1' }", fileName);
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            var fileName = TestHelper.CreateFile(TestName, "wrong file format");
            new Action(() =>
            {
                using (var jfs = new JsonFileSource(fileName))
                    jfs.Get();
            }).Should().Throw<Exception>();
        }

        [Test]
        public void Should_return_last_read_value_if_exception_was_thrown_on_next_read_and_has_no_observers()
        {
            var fileName = TestHelper.CreateFile(TestName, "{ 'Key': 'value' }");
            using (var jfs = new JsonFileSource(fileName))
            {
                var result = jfs.Get();
                result["Key"].Value.Should().Be("value");

                TestHelper.CreateFile(TestName, "wrong file format", fileName);
                Thread.Sleep(300.Milliseconds());

                result = jfs.Get();
                result["Key"].Value.Should().Be("value");
            }
        }
    }
}