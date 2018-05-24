using System;
using System.Collections.Specialized;
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
        private const string TestFileName = "test_JsonFileSource.json";

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

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var jfs = new JsonFileSource(TestFileName))
                jfs.Get().Should().BeNull();
        }

        [Test]
        public void Should_parse_String_value()
        {
            CreateTextFile("{ \"StringValue\": \"string\" }");

            using (var jfs = new JsonFileSource(TestFileName))
                jfs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "StringValue", new RawSettings("string") }
                        }));
        }

        [Test, Explicit("Not stable on mass tests")]
        public void Should_Observe_file()
        {
            new Action(() => ShouldObserveFileTest().Should().Be(2)).ShouldPassIn(1.Seconds());
        }

        private int ShouldObserveFileTest()
        {
            var val = 0;
            using (var jfs = new JsonFileSource(TestFileName))
            {
                var sub1 = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new OrderedDictionary
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
                            new OrderedDictionary
                            {
                                {"Param2", new RawSettings("set2")}
                            }));
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

        public int ShouldNotObserveFileTwiceTest_ReturnsCountOfReceives()
        {
            var val = 0;
            CreateTextFile("{ \"Param1\": \"set1\" }");

            using (var jfs = new JsonFileSource(TestFileName))
            {
                var sub = jfs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new OrderedDictionary
                            {
                                {"Param1", new RawSettings("set1")}
                            }));
                });

                CreateTextFile("{ \"Param1\": \"set1\" }");
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }
    }
}