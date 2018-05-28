using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class ScopedSource_Tests
    {
        private const string TestFileName1 = "test_ScopedSource_1.json";
        private const string TestFileName2 = "test_ScopedSource_2.json";
        private const string TestFileName3 = "test_ScopedSource_3.json";
        private const string TestFileName4 = "test_ScopedSource_4.json";
        private const string TestFileName5 = "test_ScopedSource_5.json";

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
                case 1: name = TestFileName1; break;
                case 2: name = TestFileName2; break;
                case 3: name = TestFileName3; break;
                case 4: name = TestFileName4; break;
                case 5: name = TestFileName5; break;
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
        public void Should_return_full_tree_by_source()
        {
            DeleteFiles();
            CreateTextFile("{ \"value\": 1 }");
            using (var jfs = new JsonFileSource(TestFileName1))
            using (var ss = new ScopedSource(jfs))
            {
                var result = ss.Get();
                result["value"].Value.Should().Be("1");
            }
        }

        [Test]
        public void Should_return_full_tree_by_tree()
        {
            DeleteFiles();
            CreateTextFile("{ \"value\": 1 }");

            var tree = new RawSettings(new OrderedDictionary
            {
                ["value"] = new RawSettings("1"),
            });

            using (var ss = new ScopedSource(tree))
            {
                var result = ss.Get();
                result["value"].Value.Should().Be("1");
            }
        }

        [Test]
        public void Should_scope_by_dictionaries_keys()
        {
            DeleteFiles();
            CreateTextFile("{ \"value 1\": { \"value 2\": { \"value 3\": 1 } } }", 2);
            using (var jfs = new JsonFileSource(TestFileName2))
            {
                using (var ss = new ScopedSource(jfs, "value 1", "value 2"))
                {
                    var result = ss.Get();
                    result["value 3"].Value.Should().Be("1");
                }

                using (var ss = new ScopedSource(jfs, "value 1", "value 2", "value 3"))
                {
                    var result = ss.Get();
                    result.Value.Should().Be("1");
                }
            }
        }

        [Test]
        public void Should_scope_by_list_indexes()
        {
            CreateTextFile("{ \"value\": [[1,2], [3,4,5]] }", 3);
            using (var jfs = new JsonFileSource(TestFileName3))
            {
                using (var ss = new ScopedSource(jfs, "value", "[0]"))
                {
                    var result = ss.Get();
                    result.Children.First().Value.Should().Be("1");
                    result.Children.Last().Value.Should().Be("2");
                }

                using (var ss = new ScopedSource(jfs, "value", "[1]", "[2]"))
                {
                    var result = ss.Get();
                    result.Value.Should().Be("5");
                }
            }
        }

        [Test]
        public void Should_return_null()
        {
            CreateTextFile("{ \"value\": { \"list\": [1,2] } }", 4);
            using (var jfs = new JsonFileSource(TestFileName4))
            {
                using (var ss = new ScopedSource(jfs, "unknown value"))
                    ss.Get().Should().BeNull();
                using (var ss = new ScopedSource(jfs, "value", "[0]"))
                    ss.Get().Should().BeNull();
                using (var ss = new ScopedSource(jfs, "value", "list", "[]"))
                    ss.Get().Should().BeNull();
                using (var ss = new ScopedSource(jfs, "value", "list", "[not_a_number]"))
                    ss.Get().Should().BeNull();
                using (var ss = new ScopedSource(jfs, "value", "list", "[100]"))
                    ss.Get().Should().BeNull();
            }
        }

        [Test]
        public void Should_observe_file()
        {
            List<IRawSettings> result = null;
            new Action(() => result = ShouldObserveFileTest_ReturnsReceivedSubtrees()).ShouldPassIn(1.Seconds());
            result.First().Value.Should().Be("2");
            result.Last().Value.Should().Be("4");
        }

        private List<IRawSettings> ShouldObserveFileTest_ReturnsReceivedSubtrees()
        {
            CreateTextFile("{ \"value\": { \"list\": [1,2] } }", 5);
            using (var jfs = new JsonFileSource(TestFileName5))
            {
                var rsList = new List<IRawSettings>();

                using (var ss = new ScopedSource(jfs, "value", "list", "[1]"))
                {
                    var sub = ss.Observe().Subscribe(settings => rsList.Add(settings));

                    Thread.Sleep(200.Milliseconds());
                    CreateTextFile("{ \"value\": { \"list\": [3,4,5] } }", 5);
                    Thread.Sleep(200.Milliseconds());

                    sub.Dispose();
                }

               return rsList;
            }
        }
    }
}