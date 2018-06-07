using System.Collections.Specialized;
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
            Equals(null, new SettingsNode(null, "")).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_value()
        {
            var sets1 = new SettingsNode("val 1");
            var sets2 = new SettingsNode("val 2");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_dictionaries()
        {
            var sets1 = new SettingsNode(new OrderedDictionary());
            var sets2 = new SettingsNode(null, "");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_keys()
        {
            var sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") }
                });
            var sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 2", new SettingsNode("val 1") }
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                });
            sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                    { "key 2", new SettingsNode("val 2") },
                });
            Equals(sets1, sets2).Should().BeFalse();

            sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                    { "key 2", new SettingsNode("val 2") },
                });
            sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_dictionary_values()
        {
            var sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                    { "key 2", new SettingsNode("val 2") },
                });
            var sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    { "key 1", new SettingsNode("val 1") },
                    { "key 2", new SettingsNode("val _") },
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_lists()
        {
            var sets1 = new SettingsNode(new OrderedDictionary());
            var sets2 = new SettingsNode(null, "");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_names()
        {
            var sets1 = new SettingsNode("value", "name");
            var sets2 = new SettingsNode("value", "name_");
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_sizes()
        {
            var sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode("val 1"),
                    ["2"] = new SettingsNode("val 2"),
                });
            var sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode("val 1"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_lists_values()
        {
            var sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode("val 1"),
                    ["2"] = new SettingsNode("val 2"),
                });
            var sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode("val 1"),
                    ["2"] = new SettingsNode("val _"),
                });
            Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_true_for_trees()
        {
            var sets1 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode(
                        new OrderedDictionary
                        {
                            { "key 1", new SettingsNode("val 1") },
                            { "key 2", new SettingsNode("val 2") },
                        }),
                    ["2"] = new SettingsNode(
                        new OrderedDictionary
                        {
                            { "key 3", new SettingsNode("val 3") },
                            { "key 4", new SettingsNode(
                                new OrderedDictionary
                                {
                                    ["1"] = new SettingsNode("x1"),
                                    ["2"] = new SettingsNode("x2"),
                                }) },
                        }),
                    ["3"] = new SettingsNode("5"),
                });
            var sets2 = new SettingsNode(
                new OrderedDictionary
                {
                    ["1"] = new SettingsNode(
                        new OrderedDictionary
                        {
                            { "key 1", new SettingsNode("val 1") },
                            { "key 2", new SettingsNode("val 2") },
                        }),
                    ["2"] = new SettingsNode(
                        new OrderedDictionary
                        {
                            { "key 3", new SettingsNode("val 3") },
                            { "key 4", new SettingsNode(
                                new OrderedDictionary
                                {
                                    ["1"] = new SettingsNode("x1"),
                                    ["2"] = new SettingsNode("x2"),
                                }) },
                        }),
                    ["3"] = new SettingsNode("5"),
                });
            Equals(sets1, sets2).Should().BeTrue();
        }

        [Test]
        public void Hashes_should_be_equal_for_equal_instances()
        {
            var sets1 = new SettingsNode("qwe");
            var sets2 = new SettingsNode("qwe");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new SettingsNode(new OrderedDictionary { {"qwe", new SettingsNode("ewq")} });
            sets2 = new SettingsNode(new OrderedDictionary { {"qwe", new SettingsNode("ewq")} });
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());

            sets1 = new SettingsNode(
                new OrderedDictionary { { "qwe", new SettingsNode("ewq") } },
                "name",
                "str");
            sets2 = new SettingsNode(
                new OrderedDictionary { { "qwe", new SettingsNode("ewq") } },
                "name",
                "str");
            sets1.GetHashCode().Should().Be(sets2.GetHashCode());
        }

        [Test]
        public void Hashes_should_not_be_equal_for_not_equal_instances()
        {
            var sets1 = new SettingsNode("qwe");
            var sets2 = new SettingsNode("qwe_");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new SettingsNode("qwe", "name");
            sets2 = new SettingsNode("qwe", "name_");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new SettingsNode(new OrderedDictionary { {"qwe", new SettingsNode("ewq")} });
            sets2 = new SettingsNode(new OrderedDictionary { {"qwe", new SettingsNode("ewq_")} });
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());

            sets1 = new SettingsNode(
                new OrderedDictionary { { "qwe", new SettingsNode("ewq") } },
                "name",
                "str");
            sets2 = new SettingsNode(
                new OrderedDictionary { { "qwe_", new SettingsNode("ewq") } },
                "name",
                "str");
            sets1.GetHashCode().Should().NotBe(sets2.GetHashCode());
        }
    }
}