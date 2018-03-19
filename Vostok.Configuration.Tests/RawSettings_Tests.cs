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
            RawSettings.Equals(null, new RawSettings()).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_value()
        {
            var sets1 = new RawSettings("val 1");
            var sets2 = new RawSettings("val 2");
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_dictionaries()
        {
            var sets1 = new RawSettings();
            sets1.CreateDictionary();
            var sets2 = new RawSettings();
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
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
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
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
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_false_by_existence_of_lists()
        {
            var sets1 = new RawSettings();
            sets1.CreateList();
            var sets2 = new RawSettings();
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
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
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
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
            RawSettings.Equals(sets1, sets2).Should().BeFalse();
        }

        [Test]
        public void Equals_returns_true_if_nulls()
        {
            RawSettings.Equals(null, null).Should().BeTrue();
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
            RawSettings.Equals(sets1, sets2).Should().BeTrue();
        }
    }
}