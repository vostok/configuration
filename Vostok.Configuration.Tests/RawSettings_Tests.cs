/*using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class RawSettings_Tests
    {
        [Test]
        public void Equals_returns_false_by_null_in_params()
        {
            Equals(null, new RawSettings(null, "")).Should().BeFalse();
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
            var sets1 = new RawSettings(new OrderedDictionary());
            var sets2 = new RawSettings(null, "");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_keys()
        {
            var sets1 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") }
                });
            var sets2 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 2", new RawSettings("val 1") }
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                });
            sets2 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            sets2 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_values()
        {
            var sets1 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val 2") },
                });
            var sets2 = new RawSettings(
                new OrderedDictionary
                {
                    { "key 1", new RawSettings("val 1") },
                    { "key 2", new RawSettings("val _") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_lists()
        {
            var sets1 = new RawSettings(new OrderedDictionary());
            var sets2 = new RawSettings(null, "");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_names()
        {
            var sets1 = new RawSettings("value", "name");
            var sets2 = new RawSettings("value", "name_");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_sizes()
        {
            var sets1 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings("val 1"),
                    ["2"] = new RawSettings("val 2"),
                });
            var sets2 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings("val 1"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_values()
        {
            var sets1 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings("val 1"),
                    ["2"] = new RawSettings("val 2"),
                });
            var sets2 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings("val 1"),
                    ["2"] = new RawSettings("val _"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_true_for_trees()
        {
            var sets1 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings(
                        new OrderedDictionary
                        {
                            { "key 1", new RawSettings("val 1") },
                            { "key 2", new RawSettings("val 2") },
                        }),
                    ["2"] = new RawSettings(
                        new OrderedDictionary
                        {
                            { "key 3", new RawSettings("val 3") },
                            { "key 4", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["1"] = new RawSettings("x1"),
                                    ["2"] = new RawSettings("x2"),
                                }) },
                        }),
                    ["3"] = new RawSettings("5"),
                });
            var sets2 = new RawSettings(
                new OrderedDictionary
                {
                    ["1"] = new RawSettings(
                        new OrderedDictionary
                        {
                            { "key 1", new RawSettings("val 1") },
                            { "key 2", new RawSettings("val 2") },
                        }),
                    ["2"] = new RawSettings(
                        new OrderedDictionary
                        {
                            { "key 3", new RawSettings("val 3") },
                            { "key 4", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["1"] = new RawSettings("x1"),
                                    ["2"] = new RawSettings("x2"),
                                }) },
                        }),
                    ["3"] = new RawSettings("5"),
                });
            Equals(sets1, sets2).Should().BeTrue();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = new RawSettings("qwe");
            var sets2 = new RawSettings("qwe");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new RawSettings(new OrderedDictionary { {"qwe", new RawSettings("ewq")} });
            sets2 = new RawSettings(new OrderedDictionary { {"qwe", new RawSettings("ewq")} });
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new RawSettings(
                new OrderedDictionary { { "qwe", new RawSettings("ewq") } },
                "name",
                "str");
            sets2 = new RawSettings(
                new OrderedDictionary { { "qwe", new RawSettings("ewq") } },
                "name",
                "str");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());
        }

        [Test]
        public void Hashes_should_not_be_equal_for_not_equal_instances()
        {
            var sets1 = new RawSettings("qwe");
            var sets2 = new RawSettings("qwe_");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings("qwe", "name");
            sets2 = new RawSettings("qwe", "name_");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings(new OrderedDictionary { {"qwe", new RawSettings("ewq")} });
            sets2 = new RawSettings(new OrderedDictionary { {"qwe", new RawSettings("ewq_")} });
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new RawSettings(
                new OrderedDictionary { { "qwe", new RawSettings("ewq") } },
                "name",
                "str");
            sets2 = new RawSettings(
                new OrderedDictionary { { "qwe_", new RawSettings("ewq") } },
                "name",
                "str");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());
        }

        [Test]
        public void Keys_should_be_case_insensitive()
        {
            var sets = new RawSettings(
                new OrderedDictionary(new ChildrenKeysComparer())
                {
                    ["qwe"] = new RawSettings("v0"),
                    ["QWE"] = new RawSettings("v1"),    //rewrites
                    ["TeSt"] = new RawSettings("v2"),
                });

            sets.Children.Count().Should().Be(2, "v0 was rewrited");

            sets["qwe"].Value.Should().Be("v1");
            sets["QWE"].Value.Should().Be("v1");
            sets["TEST"].Value.Should().Be("v2");
            sets["test"].Value.Should().Be("v2");
        }
    }
}*/