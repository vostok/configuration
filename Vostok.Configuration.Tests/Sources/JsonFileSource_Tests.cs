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
    public class JsonFileSource_Tests
    {
        private const string TestFileName1 = "test_JsonFileSource_1.json";
        private const string TestFileName2 = "test_JsonFileSource_2.json";
        private const string TestFileName3 = "test_JsonFileSource_3.json";
        private const string TestFileName4 = "test_JsonFileSource_4.json";
        private const string TestFileName5 = "test_JsonFileSource_5.json";

        [SetUp]
        public void SetUp()
        {
            Cleanup();
        }


        [TearDown]
        public void Cleanup()
        {
            DeleteFiles();
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
                case 5: name = TestFileName5;   break;
            }

            using (var file = new StreamWriter(name, false))
                file.WriteLine(text);
        }

        private static void DeleteFiles()
        {
            File.Delete(TestFileName1);
            File.Delete(TestFileName2);
            File.Delete(TestFileName3);
            File.Delete(TestFileName4);
            File.Delete(TestFileName5);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var jfs = new JsonFileSource("some_file"))
                jfs.Get().Should().BeNull();
        }

        [Test]
        [Order(1)]
        public void Should_parse_String_value()
        {
            CreateTextFile("{ \"StringValue\": \"string\" }");

            using (var jfs = new JsonFileSource(TestFileName1))
            {
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");
            }

            DeleteFiles();
        }

        [Test]
        public void Should_Get_file_updates()
        {
            CreateTextFile("{ \"StringValue\": \"string\" }", 3);

            using (var jfs = new JsonFileSource(TestFileName3))
            {
                var result = jfs.Get();
                result["StringValue"].Value.Should().Be("string");

                CreateTextFile("{ \"StringValue\": \"string2\" }", 3);
                Thread.Sleep(300.Milliseconds());

                result = jfs.Get();
                result["StringValue"].Value.Should().Be("string2");
            }

            DeleteFiles();
        }

        [Test, Explicit("Not stable on mass tests")]
        public void Should_Observe_file()
        {
            new Action(() => ShouldObserveFileTest().Should().Be(2)).ShouldPassIn(1.Seconds());
            DeleteFiles();
        }

        private int ShouldObserveFileTest()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName5))
            {
                var sub1 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param2"].Value.Should().Be("set2");
                });

                CreateTextFile("{ \"Param2\": \"set2\" }", 5);

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
            DeleteFiles();
        }

        private int ShouldNotObserveFileTwiceTest_ReturnsCountOfReceives()
        {
            var val = 0;
            CreateTextFile("{ \"Param1\": \"set1\" }");

            using (var jfs = new JsonFileSource(TestFileName1))
            {
                var sub = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["Param"].Value.Should().Be("set1");
                });

                CreateTextFile("{ \"Param1\": \"set1\" }");
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }

        [Test]
        [Order(3)]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            CreateTextFile("wrong file format", 2);
            new Action(() =>
            {
                using (var jfs = new JsonFileSource(TestFileName2))
                    jfs.Get();
            }).Should().Throw<Exception>();
        }

        [Test]
        public void Should_return_last_read_value_if_exception_was_thrown_on_next_read_and_has_no_observers()
        {
            CreateTextFile("{ \"Key\": \"value\" }", 4);
            using (var jfs = new JsonFileSource(TestFileName4))
            {
                var result = jfs.Get();
                result["Key"].Value.Should().Be("value");

                CreateTextFile("wrong file format", 4);
                Thread.Sleep(300.Milliseconds());

                result = jfs.Get();
                result["Key"].Value.Should().Be("value");
            }
        }
    }
}