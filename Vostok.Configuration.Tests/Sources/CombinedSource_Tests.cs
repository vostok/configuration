﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.MergeOptions;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class CombinedSource_Tests
    {
        private SingleFileWatcherSubstitute[] watchers;

        private CombinedSource CreateCombinedSource(string[] filesContent, SettingsMergeOptions options = null)
        {
            if (filesContent == null || !filesContent.Any())
                return new CombinedSource();

            watchers = new SingleFileWatcherSubstitute[filesContent.Length];
            var list = filesContent.Select((n, i) => new JsonFileSource($"{i}.json", f =>
                {
                    watchers[i] = new SingleFileWatcherSubstitute(f);
                    watchers[i].GetUpdate(n);
                    return watchers[i];
                })).ToList();
            return new CombinedSource(list, options);
        }

        [Test]
        public void Should_return_null_if_no_sources()
        {
            using (var cs = CreateCombinedSource(null))
                cs.Get().Should().BeNull();
        }

        [Test]
        public void Should_return_alone_source()
        {
            var filesContent = new[] { "{ 'value 1': 'string 1' }" };

            using (var cs = CreateCombinedSource(filesContent))
            {
                var result = cs.Get();
                result["value 1"].Value.Should().Be("string 1");
            }
        }

        [Test]
        public void Should_merge_sources_with_override_values()
        {
            var filesContent = new[]
            {
                "{ 'value 1': 'string 1' }",
                "{ 'value 2': 'string 2' }",
                "{ 'value 2': 'string 22' }",
            };

            using (var cs = CreateCombinedSource(filesContent, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Shallow }))
            {
                var result = cs.Get();
                result["value 2"].Value.Should().Be("string 22");
            }
            using (var cs = CreateCombinedSource(filesContent, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Deep }))
            {
                var result = cs.Get();
                result["value 1"].Value.Should().Be("string 1");
                result["value 2"].Value.Should().Be("string 22");
            }
        }

        [Test]
        public void Should_observe_file()
        {
            new Action(() => ShouldObserveFileTest_ReturnsCountOfReceives().Should().Be(2)).ShouldPassIn(1.Seconds());
        }

        private int ShouldObserveFileTest_ReturnsCountOfReceives()
        {
            var filesContent = new[]
            {
                "{ 'value 1': 1, 'list': [1,2] }",
                "{ 'value 2': 2 }",
            };
            var val = 0;

            using (var ccs = CreateCombinedSource(filesContent, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Deep }))
            {
                var sub = ccs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["value 1"].Value.Should().Be("1");
                    settings["value 2"].Value.Should().Be("2");
                    settings["list"].Children.Select(c => c.Value).Should().ContainInOrder("1", "2");
                });

                //update file
                Task.Run(() =>
                {
                    Thread.Sleep(20);
                    watchers[1].GetUpdate("{ 'value 2': 2, 'list': [3,4] }");
                });
                Thread.Sleep(50.Milliseconds());

                sub.Dispose();
            }
            return val;
        }
    }
}