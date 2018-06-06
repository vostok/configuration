using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
    public class ScopedSource_Tests
    {
        private const string TestName = nameof(ScopedSource);

        [TearDown]
        public void Cleanup()
        {
            TestHelper.DeleteAllFiles(TestName);
        }

        [Test]
        public void Should_return_full_tree_by_source()
        {
            var fileName = TestHelper.CreateFile(TestName, "{ \"value\": 1 }");
            using (var jfs = new JsonFileSource(fileName))
            using (var ss = new ScopedSource(jfs))
            {
                var result = ss.Get();
                result["value"].Value.Should().Be("1");
            }
        }

        [Test]
        public void Should_return_full_tree_by_tree()
        {
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
            var fileName = TestHelper.CreateFile(TestName, "{ \"value 1\": { \"value 2\": { \"value 3\": 1 } } }");
            using (var jfs = new JsonFileSource(fileName))
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
            var fileName = TestHelper.CreateFile(TestName, "{ \"value\": [[1,2], [3,4,5]] }");
            using (var jfs = new JsonFileSource(fileName))
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
            var fileName = TestHelper.CreateFile(TestName, "{ \"value\": { \"list\": [1,2] } }");
            using (var jfs = new JsonFileSource(fileName))
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

        //todo: fails sometimes
        [Test]
        public void Should_observe_file()
        {
            List<IRawSettings> result = null;
            new Action(() => result = ShouldObserveFileTest_ReturnsReceivedSubtrees()).ShouldPassIn(1.Seconds());
            result.Select(r => r.Value).Should().Equal("2", "4");
        }

        private List<IRawSettings> ShouldObserveFileTest_ReturnsReceivedSubtrees()
        {
            var fileName = TestHelper.CreateFile(TestName, "{ \"value\": { \"list\": [1,2] } }");
            using (var jfs = new JsonFileSource(fileName))
            {
                var rsList = new List<IRawSettings>();

                using (var ss = new ScopedSource(jfs, "value", "list", "[1]"))
                {
                    var sub = ss.Observe().Subscribe(settings => rsList.Add(settings));

                    Thread.Sleep(200.Milliseconds());
                    TestHelper.CreateFile(TestName, "{ \"value\": { \"list\": [3,4,5] } }", fileName);
                    Thread.Sleep(200.Milliseconds());

                    sub.Dispose();
                }

               return rsList;
            }
        }
    }
}