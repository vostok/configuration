using System;
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
    public class CombinedSource_Tests: Sources_Test
    {
        private SingleFileWatcherSubstitute[] watchers;

        private CombinedSource CreateCombinedSource(string[] filesContent, SettingsMergeOptions options = null)
        {
            if (filesContent == null || !filesContent.Any())
                return new CombinedSource();

            watchers = new SingleFileWatcherSubstitute[filesContent.Length];
            var list = filesContent.Select((n, i) => new JsonFileSource($"{i}.json", (f, e) =>
                {
                    watchers[i] = new SingleFileWatcherSubstitute(f, e);
                    watchers[i].GetUpdate(n);
                    return watchers[i];
                })).ToList();
            return new CombinedSource(list, options);
        }

        [Test]
        public void Should_throw_exception_if_no_sources()
        {
            new Action(() => new CombinedSource(null))
                .Should().Throw<ArgumentException>();

            new Action(() => new CombinedSource(new JsonFileSource[0]))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_return_alone_source()
        {
            var filesContent = new[] { "{ 'value 1': 'string 1' }" };

            var cs = CreateCombinedSource(filesContent);
            var result = cs.Get();
            result["value 1"].Value.Should().Be("string 1");
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

            var cs = CreateCombinedSource(filesContent, new SettingsMergeOptions {ObjectMergeStyle = ObjectMergeStyle.Shallow});
            var result = cs.Get();
            result["value 2"].Value.Should().Be("string 22");

            cs = CreateCombinedSource(filesContent, new SettingsMergeOptions {ObjectMergeStyle = ObjectMergeStyle.Deep});
            result = cs.Get();
            result["value 1"].Value.Should().Be("string 1");
            result["value 2"].Value.Should().Be("string 22");
        }

        [Test]
        public void Should_observe_file()
        {
            var res = 0;
            new Action(() => res = ShouldObserveFileTest_ReturnsCountOfReceives()).ShouldPassIn(1.Seconds());
            res.Should().Be(2);
        }

        private int ShouldObserveFileTest_ReturnsCountOfReceives()
        {
            var filesContent = new[]
            {
                "{ 'value 1': 1, 'list': [1,2] }",
                "{ 'value 2': 2 }",
            };
            var val = 0;

            var ccs = CreateCombinedSource(filesContent, new SettingsMergeOptions {ObjectMergeStyle = ObjectMergeStyle.Deep});
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
            return val;
        }

        [Test]
        public void Should_return_OnError_to_subscriber_in_case_of_exception_and_continue_work_after_resubscription()
        {
            var filesContent = new[]
            {
                "wrong file format",
            };

            var cs = CreateCombinedSource(filesContent);
            var onNext = 0;
            var onError = 0;

            cs.Observe().Subscribe(node => onNext++, e => onError++);
            Thread.Sleep(50);

            onNext.Should().Be(0);
            onError.Should().Be(1);



            //update file
            Task.Run(() =>
            {
                Thread.Sleep(20);
                watchers[0].GetUpdate("{ 'value': 123 }");
            });
            Thread.Sleep(50);

            onNext.Should().Be(0, "need resubscription for changes");



            cs.Observe().Subscribe(node => onNext++, e => onError++);
            Thread.Sleep(50);

            onNext.Should().Be(1);
            onError.Should().Be(1);
        }
    }
}