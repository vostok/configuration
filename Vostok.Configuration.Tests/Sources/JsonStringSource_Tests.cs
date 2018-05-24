using System;
using System.Collections.Specialized;
using System.Globalization;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonStringSource_Tests
    {
        [Test]
        public void Should_return_null_if_null_or_whitespace_string()
        {
            using (var jss = new JsonStringSource(null))
                jss.Get().Should().BeNull();
            using (var jss = new JsonStringSource(" "))
                jss.Get().Should().BeNull();
        }

        [Test]
        public void Should_parse_String_value()
        {
            const string value = "{ \"StringValue\": \"string\" }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "StringValue", new RawSettings("string", "StringValue") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_Integer_value()
        {
            const string value = "{ \"IntValue\": 123 }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "IntValue", new RawSettings("123", "IntValue") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_Double_value()
        {
            const string value = "{ \"DoubleValue\": 123.321 }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "DoubleValue", new RawSettings(123.321d.ToString(CultureInfo.CurrentCulture), "DoubleValue") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_Boolean_value()
        {
            const string value = "{ \"BooleanValue\": true }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "BooleanValue", new RawSettings("True", "BooleanValue") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_Null_value()
        {
            const string value = "{ \"NullValue\": null }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "NullValue", new RawSettings(null, "NullValue") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_Array_value()
        {
            const string value = "{ \"IntArray\": [1, 2, 3] }";

            using (var jss = new JsonStringSource(value))
            {
                var result = jss.Get();
                result.Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "IntArray", new RawSettings(new OrderedDictionary
                            {
                                [(object)0] = new RawSettings("1", "0"),
                                [(object)1] = new RawSettings("2", "1"),
                                [(object)2] = new RawSettings("3", "2"),
                            }, "IntArray") }
                        }, "root"));
            }
        }

        [Test]
        public void Should_parse_Object_value()
        {
            const string value = "{ \"Object\": { \"StringValue\": \"str\" } }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "Object", new RawSettings(
                                new OrderedDictionary
                                {
                                    { "StringValue", new RawSettings("str", "StringValue") }
                                }, "Object") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_ArrayOfObjects_value()
        {
            const string value = "{ \"Array\": [{ \"StringValue\": \"str\" }, { \"IntValue\": 123 }] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "Array", new RawSettings(
                                new OrderedDictionary
                                {
                                    [(object)0] = new RawSettings(new OrderedDictionary
                                    {
                                        {"StringValue", new RawSettings("str", "StringValue")}
                                    }, "0"),
                                    [(object)1] = new RawSettings(new OrderedDictionary
                                    {
                                        {"IntValue", new RawSettings("123", "IntValue")}
                                    }, "1"),
                                }, "Array") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_ArrayOfNulls_value()
        {
            const string value = "{ \"Array\": [null, null] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "Array", new RawSettings(
                                new OrderedDictionary
                                {
                                    [(object)0] = new RawSettings(null, "0"),
                                    [(object)1] = new RawSettings(null, "1")
                                }, "Array") }
                        }, "root"));
        }

        [Test]
        public void Should_parse_ArrayOfArrays_value()
        {
            const string value = "{ \"Array\": [[\"s\", \"t\"], [\"r\"]] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "Array", new RawSettings(
                                new OrderedDictionary
                                {
                                    [(object)0] = new RawSettings(new OrderedDictionary
                                    {
                                        [(object)0] = new RawSettings("s", "0"),
                                        [(object)1] = new RawSettings("t", "1"),
                                    }, "0"),
                                    [(object)1] = new RawSettings(new OrderedDictionary
                                    {
                                        [(object)0] = new RawSettings("r", "0"),
                                    }, "1")
                                }, "Array") }
                        }, "root"));
        }

        [Test]
        public void Should_subscribe_and_get_parsed_tree()
        {
            new Action(() => ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives()
        {
            const string value = "{ \"IntValue\": 123 }";
            var val = 0;

            using (var jss = new JsonStringSource(value))
            {
                var sub = jss.Observe().Subscribe(
                    settings =>
                    {
                        val++;
                        settings.Should().BeEquivalentTo(
                            new RawSettings(
                                new OrderedDictionary
                                {
                                    {"IntValue", new RawSettings("123", "IntValue")}
                                }));
                    });
                sub.Dispose();
            }

            return val;
        }
    }
}