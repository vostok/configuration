using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class RawSettings_Tests
    {
        [Test]
        public void Equals_returns_false_if_one_of_params_is_null()
        {
            Equals(null, new RawSettings(null)).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_value()
        {
            var sets1 = new RawSettings("val 1");
            var sets2 = new RawSettings("val 2");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_dictionaries()
        {
            var sets1 = new RawSettings(new Dictionary<string, RawSettings>());
            var sets2 = new RawSettings(null);
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_keys()
        {
            var sets1 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") }
                });
            var sets2 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 2", new RawSettings("val 1") }
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                });
            sets2 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            sets2 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_values()
        {
            var sets1 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            var sets2 = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val _") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_lists()
        {
            var sets1 = new RawSettings(new List<RawSettings>());
            var sets2 = new RawSettings(null);
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_sizes()
        {
            var sets1 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings("val 1"),
                    new RawSettings("val 2"),
                });
            var sets2 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings("val 1"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_values()
        {
            var sets1 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings("val 1"),
                    new RawSettings("val 2"),
                });
            var sets2 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings("val 1"),
                    new RawSettings("val _"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_true_for_trees()
        {
            var sets1 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "key 1", new RawSettings("val 1") },
                            { "key 2", new RawSettings("val 2") },
                        }),
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "key 3", new RawSettings("val 3") },
                            { "key 4", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings("x1"),
                                    new RawSettings("x2"),
                                }) },
                        }),
                    new RawSettings("5"),
                });
            var sets2 = new RawSettings(
                new List<RawSettings>
                {
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "key 1", new RawSettings("val 1") },
                            { "key 2", new RawSettings("val 2") },
                        }),
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "key 3", new RawSettings("val 3") },
                            { "key 4", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings("x1"),
                                    new RawSettings("x2"),
                                }) },
                        }),
                    new RawSettings("5"),
                });
            Equals(sets1, sets2).Should().BeTrue();
        }

        [Test]
        public void Hashes_should_be_equal()
        {
            var sets1 = new RawSettings("qwe");
            var sets2 = new RawSettings("qwe");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new RawSettings(new Dictionary<string, RawSettings> { {"qwe", new RawSettings("ewq")} });
            sets2 = new RawSettings(new Dictionary<string, RawSettings> { {"qwe", new RawSettings("ewq")} });
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new RawSettings(new List<RawSettings> { new RawSettings("1"), new RawSettings("2") });
            sets2 = new RawSettings(new List<RawSettings> { new RawSettings("1"), new RawSettings("2") });
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new RawSettings(
                new Dictionary<string, RawSettings> { { "qwe", new RawSettings("ewq") } },
                new List<RawSettings> { new RawSettings("1"), new RawSettings("2") },
                "str");
            sets2 = new RawSettings(
                new Dictionary<string, RawSettings> { { "qwe", new RawSettings("ewq") } },
                new List<RawSettings> { new RawSettings("1"), new RawSettings("2") },
                "str");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());
        }

        [Test]
        public void Hashes_should_not_be_equal()
        {
            var sets1 = new RawSettings("qwe");
            var sets2 = new RawSettings("qwe_");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings(new Dictionary<string, RawSettings> { {"qwe", new RawSettings("ewq")} });
            sets2 = new RawSettings(new Dictionary<string, RawSettings> { {"qwe", new RawSettings("ewq_")} });
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings(new List<RawSettings> { new RawSettings("1"), new RawSettings("2") });
            sets2 = new RawSettings(new List<RawSettings> { new RawSettings("1"), new RawSettings("_") });
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings(
                new Dictionary<string, RawSettings> { { "qwe", new RawSettings("ewq") } },
                new List<RawSettings> { new RawSettings("1"), new RawSettings("2") },
                "str");
            sets2 = new RawSettings(
                new Dictionary<string, RawSettings> { { "qwe_", new RawSettings("ewq") } },
                new List<RawSettings> { new RawSettings("1"), new RawSettings("2") },
                "str");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());
        }
    }
}