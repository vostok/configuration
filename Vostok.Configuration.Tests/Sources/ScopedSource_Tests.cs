using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.SettingsTree;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class ScopedSource_Tests
    {
        [Test]
        public void Should_return_full_tree_by_source()
        {
            const string fileName = "test.json";
            const string content = "{ 'value': 1 }";

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                var watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
            using (var ss = new ScopedSource(jfs))
            {
                var result = ss.Get();
                result["value"].Value.Should().Be("1");
            }
        }

        [Test]
        public void Should_return_full_tree_by_tree()
        {
            var tree = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["value"] = new ValueNode("1"),
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
            const string fileName = "test.json";
            const string content = "{ 'value 1': { 'value 2': { 'value 3': 1 } } }";

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                var watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
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
            const string fileName = "test.json";
            const string content = "{ 'value': [[1,2], [3,4,5]] }";

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                var watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
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
            const string fileName = "test.json";
            const string content = "{ 'value': { 'list': [1,2] } }";

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                var watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
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
            List<ISettingsNode> result = null;
            new Action(() => result = ShouldObserveFileTest_ReturnsReceivedSubtrees()).ShouldPassIn(1.Seconds());
            result.Select(r => r.Value).Should().Equal("2", "4");
        }

        private List<ISettingsNode> ShouldObserveFileTest_ReturnsReceivedSubtrees()
        {
            const string fileName = "test.json";
            var content = "{ 'value': { 'list': [1,2] } }";
            SingleFileWatcherSubstitute watcher = null;

            using (var jfs = new JsonFileSource(fileName, f =>
            {
                watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            }))
            {
                var rsList = new List<ISettingsNode>();

                using (var ss = new ScopedSource(jfs, "value", "list", "[1]"))
                {
                    var sub = ss.Observe().Subscribe(settings => rsList.Add(settings));

                    content = "{ 'value': { 'list': [3,4,5] } }";
                    //update file
                    Task.Run(() =>
                    {
                        Thread.Sleep(50);
                        watcher.GetUpdate(content);
                    });
                    Thread.Sleep(150.Milliseconds());

                    sub.Dispose();
                }

               return rsList;
            }
        }
    }
}