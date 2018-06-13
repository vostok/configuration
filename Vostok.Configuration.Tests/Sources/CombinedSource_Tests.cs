using System;
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
    [TestFixture, SingleThreaded]
    public class CombinedSource_Tests
    {
        private const string TestName = nameof(CombinedSource);

        [TearDown]
        public void Cleanup()
        {
            TestHelper.DeleteAllFiles(TestName);
        }

        private static CombinedSource CreateCombinedSource(string[] fileNames, SettingsMergeOptions options = null)
        {
            if (fileNames == null || !fileNames.Any())
                return new CombinedSource();

            var list = fileNames.Select(n => new JsonFileSource(n)).ToList();
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
            var fileNames = new[]
            {
                TestHelper.CreateFile(TestName, "{ 'value 1': 'string 1' }"),
            };

            using (var cs = CreateCombinedSource(fileNames))
            {
                var result = cs.Get();
                result["value 1"].Value.Should().Be("string 1");
            }
        }

        [Test]
        public void Should_merge_sources_with_override_values()
        {
            var fileNames = new[]
            {
                TestHelper.CreateFile(TestName, "{ 'value 1': 'string 1' }"),
                TestHelper.CreateFile(TestName, "{ 'value 2': 'string 2' }"),
                TestHelper.CreateFile(TestName, "{ 'value 2': 'string 22' }"),
            };

            using (var cs = CreateCombinedSource(fileNames, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Shallow }))
            {
                var result = cs.Get();
                result["value 2"].Value.Should().Be("string 22");
            }
            using (var cs = CreateCombinedSource(fileNames, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Deep }))
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
            var fileNames = new[]
            {
                TestHelper.CreateFile(TestName, "{ 'value 1': 1, 'list': [1,2] }"),
                TestHelper.CreateFile(TestName, "{ 'value 2': 2 }"),
            };
            var val = 0;

            using (var ccs = CreateCombinedSource(fileNames, new SettingsMergeOptions { TreeMergeStyle = TreeMergeStyle.Deep }))
            {
                var sub = ccs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings["value 1"].Value.Should().Be("1");
                    settings["value 2"].Value.Should().Be("2");
                    settings["list"].Children.Select(c => c.Value).Should().ContainInOrder("1", "2");
                });

                TestHelper.CreateFile(TestName, "{ 'value 2': 2, 'list': [3,4] }", fileNames[1]);
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }
    }
}