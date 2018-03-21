using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Convertions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    // CR(krait): Tests are unstable, IOException occurs from time to time. Perhaps disposing the watcher after each test will fix this?
    [TestFixture]
    public class ScopedSource_Tests
    {
        private const string TestFileName = "test_ScopedSource.json";

        [SetUp]
        public void SetUp()
        {
        }

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
        public void Should_return_full_tree()
        {
            CreateTextFile("{ \"value\": 1 }");
            var jfs = new JsonFileSource(TestFileName);

            new ScopedSource(jfs).Get()
                .Should().BeEquivalentTo(new RawSettings(
                    new Dictionary<string, RawSettings>
                    {
                        { "value", new RawSettings("1") },
                    }));
        }

        [Test]
        public void Should_scope_by_dictionaries_keys()
        {
            CreateTextFile("{ \"value 1\": { \"value 2\": { \"value 3\": 1 } } }");
            var jfs = new JsonFileSource(TestFileName);

            new ScopedSource(jfs, "value 1", "value 2").Get()
                .Should().BeEquivalentTo(new RawSettings(
                    new Dictionary<string, RawSettings>
                    {
                        { "value 3", new RawSettings("1") },
                    }));

            new ScopedSource(jfs, "value 1", "value 2", "value 3").Get()
                .Should().BeEquivalentTo(new RawSettings("1"));
        }

        [Test]
        public void Should_scope_by_list_indexes()
        {
            CreateTextFile("{ \"value\": [[1,2], [3,4,5]] }");
            var jfs = new JsonFileSource(TestFileName);

            new ScopedSource(jfs, "value", "[0]").Get()
                .Should().BeEquivalentTo(new RawSettings(
                    new List<RawSettings>
                    {
                        new RawSettings("1"),
                        new RawSettings("2"),
                    }));

            new ScopedSource(jfs, "value", "[1]", "[2]").Get()
                .Should().BeEquivalentTo(new RawSettings("5"));
        }

        [Test]
        public void Should_return_null()
        {
            CreateTextFile("{ \"value\": { \"list\": [1,2] } }");
            var jfs = new JsonFileSource(TestFileName);

            new ScopedSource(jfs, "unknown value").Get().Should().BeNull();
            new ScopedSource(jfs, "value", "[0]").Get().Should().BeNull();
            new ScopedSource(jfs, "value", "list", "[]").Get().Should().BeNull();
            new ScopedSource(jfs, "value", "list", "[not_a_number]").Get().Should().BeNull();
            new ScopedSource(jfs, "value", "list", "[100]").Get().Should().BeNull();
        }

        [Test]
        public void Should_observe_file()
        {
            new Action(() => Should_observe_file_test().Should().BeEquivalentTo(
                new List<RawSettings>
                {
                    new RawSettings("2"),
                    new RawSettings("4"),
                }
            )).ShouldPassIn(3.Seconds());
        }

        // CR(krait): Helper methods are usually named in the traditional convention, camel case.
        private static List<RawSettings> Should_observe_file_test()
        {
            CreateTextFile("{ \"value\": { \"list\": [1,2] } }");
            var jfs = new JsonFileSource(TestFileName, 300.Milliseconds());
            var ss = new ScopedSource(jfs, "value", "list", "[1]");
            var rsList = new List<RawSettings>();

            var sub = ss.Observe().Subscribe(settings => rsList.Add(settings));
            // CR(krait): Nope, no more Thread.Sleep's in tests, please.
            Thread.Sleep(1.Seconds());
            CreateTextFile("{ \"value\": { \"list\": [3,4,5] } }");
            Thread.Sleep(1.Seconds());

            sub.Dispose();
            return rsList;
        }
    }
}