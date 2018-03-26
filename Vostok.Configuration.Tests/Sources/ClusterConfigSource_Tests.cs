using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class ClusterConfigSource_Tests
    {
        [Test]
        public void Should_get_all_settings()
        {
            using (var ccs = new ClusterConfigSource())
                ccs.Get().ChildrenByKey.Should().HaveCountGreaterThan(1000);
        }

        [Test]
        public void Should_get_by_prefix()
        {
            using (var ccs = new ClusterConfigSource("banana/core", null))
                ccs.Get().ChildrenByKey.Should().HaveCountGreaterThan(0).And.HaveCountLessThan(100);
        }

        [Test]
        public void Should_get_by_key_in_whole_tree()
        {
            using (var ccs = new ClusterConfigSource(" ", "banana/core/houstontimeout"))
                ccs.Get().Children.Should().HaveCount(1);
        }

        [Test]
        public void Should_get_by_prefix_and_key()
        {
            using (var ccs = new ClusterConfigSource("banana/core", "houstontimeout"))
                ccs.Get().Children.Should().HaveCount(1);
        }

        [Test]
        public void Should_throw_exception_on_wrong_key()
        {
            new Action(() =>
                {
                    using (var ccs = new ClusterConfigSource(null, "wrong key"))
                        ccs.Get();
                }).Should().Throw<ArgumentException>();
        }

        [Test, Ignore("Unable to update settings")]
        public void Should_observe_variables()
        {
            new Action(() => ShouldObserveVariablesTest().Should().Be(default)).ShouldPassIn(1.Seconds());
        }
        private int ShouldObserveVariablesTest()
        {
            return default;
        }
    }
}