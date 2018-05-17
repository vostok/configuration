using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.ClusterConfig;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class ClusterConfigSource_Tests
    {
        private IClusterConfigClientProxy clusterClient;
        private const string FullKey = "banana/core/houstontimeout";
        private const string Prefix = "banana/core";
        private const string Key = "houstontimeout";
        private const string Value = "1 minute";
        private Dictionary<string, List<string>>  fullDict;
        private Dictionary<string, List<string>>  shortDict;
        private List<string> keyList;

        [SetUp]
        public void SetUp()
        {
            fullDict = new Dictionary<string, List<string>>();
            shortDict = new Dictionary<string, List<string>>();
            keyList = new List<string>{ Value };
            for (var i = 0; i < 150; i++)
            {
                if (i < 20)
                    shortDict.Add(i.ToString(), new List<string> { "a", "b" });
                fullDict.Add(i.ToString(), new List<string>{ "a", "b" });
            }
            fullDict.Add(FullKey, keyList);
            shortDict.Add(FullKey, keyList);

            clusterClient = Substitute.For<IClusterConfigClientProxy>();
            clusterClient.GetAll().Returns(fullDict);
            clusterClient.GetByKey(Arg.Any<string>()).Returns(keyList);
            clusterClient.GetByPrefix(Arg.Any<string>()).Returns(shortDict);
        }
        
        [Test]
        public void Should_get_all_settings()
        {
            using (var ccs = new ClusterConfigSource(null, null, clusterClient, 100.Milliseconds(), true))
                ccs.Get().ChildrenByKey.Should().HaveCountGreaterThan(100);
        }

        [Test]
        public void Should_get_by_prefix()
        {
            using (var ccs = new ClusterConfigSource(Prefix, null, clusterClient, 100.Milliseconds(), true))
                ccs.Get().ChildrenByKey.Should().HaveCountGreaterThan(0).And.HaveCountLessThan(100);
        }

        [Test]
        public void Should_get_by_key_in_whole_tree()
        {
            using (var ccs = new ClusterConfigSource(" ", FullKey, clusterClient, 100.Milliseconds(), true))
                ccs.Get().Children.Should().HaveCount(1);
        }

        [Test]
        public void Should_get_by_prefix_and_key()
        {
            using (var ccs = new ClusterConfigSource(Prefix, Key, clusterClient, 100.Milliseconds(), true))
                ccs.Get().Children.Should().HaveCount(1).And.BeEquivalentTo(new RawSettings(Value));
        }

        [Test]
        public void Should_throw_exception_on_wrong_key()
        {
            new Action(() =>
            {
                using (new ClusterConfigSource(null, "wrong key", clusterClient, 100.Milliseconds(), true)) { }
            }).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_observe_variables()
        {
            new Action(() => ShouldObserveVariablesTest().Should().Be(2)).ShouldPassIn(1.Seconds());
        }
        private int ShouldObserveVariablesTest()
        {
            const string newValue = "NewValue";

            var val = 0;
            using (var ccs = new ClusterConfigSource(Prefix, Key, clusterClient, 100.Milliseconds(), true))
            {
                var sub = ccs.Observe().Subscribe(settings =>
                {
                    if (settings == null) return;
                    val++;
                    if (val == 2)
                        settings.Should().BeEquivalentTo(
                            new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(newValue),
                                }));
                });

                fullDict.Add("_", new List<string>{ "a", "b" });
                shortDict.Add("_", new List<string>{ "a", "b" });
                Thread.Sleep(200.Milliseconds());
                fullDict[FullKey][0] = newValue;
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }
    }
}