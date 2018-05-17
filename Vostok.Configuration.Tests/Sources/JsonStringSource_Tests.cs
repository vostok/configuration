using System;
using System.Collections.Generic;
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
                        new Dictionary<string, RawSettings>
                        {
                            { "StringValue", new RawSettings("string") }
                        }));
        }

        [Test]
        public void Should_parse_Integer_value()
        {
            const string value = "{ \"IntValue\": 123 }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "IntValue", new RawSettings("123") }
                        }));
        }

        [Test]
        public void Should_parse_Double_value()
        {
            const string value = "{ \"DoubleValue\": 123.321 }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "DoubleValue", new RawSettings(123.321d.ToString(CultureInfo.CurrentCulture)) }
                        }));
        }

        [Test]
        public void Should_parse_Boolean_value()
        {
            const string value = "{ \"BooleanValue\": true }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "BooleanValue", new RawSettings("True") }
                        }));
        }

        [Test]
        public void Should_parse_Null_value()
        {
            const string value = "{ \"NullValue\": null }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "NullValue", new RawSettings(null) }
                        }));
        }

        [Test]
        public void Should_parse_Array_value()
        {
            const string value = "{ \"IntArray\": [1, 2, 3] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "IntArray", new RawSettings(new List<RawSettings>
                            {
                                new RawSettings("1"),
                                new RawSettings("2"),
                                new RawSettings("3"),
                            }) }
                        }));
        }

        [Test]
        public void Should_parse_Object_value()
        {
            const string value = "{ \"Object\": { \"StringValue\": \"str\" } }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Object", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "StringValue", new RawSettings("str") }
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfObjects_value()
        {
            const string value = "{ \"Array\": [{ \"StringValue\": \"str\" }, { \"IntValue\": 123 }] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(new Dictionary<string, RawSettings>
                                    {
                                        { "StringValue", new RawSettings("str") }
                                    }),
                                    new RawSettings(new Dictionary<string, RawSettings>
                                    {
                                        { "IntValue", new RawSettings("123") }
                                    })
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfNulls_value()
        {
            const string value = "{ \"Array\": [null, null] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(null),
                                    new RawSettings(null)
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfArrays_value()
        {
            const string value = "{ \"Array\": [[\"s\", \"t\"], [\"r\"]] }";

            using (var jss = new JsonStringSource(value))
                jss.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(new List<RawSettings>
                                    {
                                        new RawSettings("s"),
                                        new RawSettings("t"),
                                    }),
                                    new RawSettings(new List<RawSettings>
                                    {
                                        new RawSettings("r"),
                                    })
                                }) }
                        }));
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
                                new Dictionary<string, RawSettings>
                                {
                                    {"IntValue", new RawSettings("123")}
                                }));
                    });
                sub.Dispose();
            }

            return val;
        }
    }
}