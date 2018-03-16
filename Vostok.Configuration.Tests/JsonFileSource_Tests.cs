using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class JsonFileSource_Tests
    {
        private const string TestFileName = "test.json";

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFileName);
        }

        private static void CreateTextFile(string text)
        {
            using (var file = new StreamWriter(TestFileName, false))
                file.WriteLine(text);
        }
        
        [Test]
        public void Should_parse_String__value()
        {
            CreateTextFile("{ \"StringValue\": \"string\" }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "StringValue", new RawSettings("string") }
                        }));
        }

        [Test]
        public void Should_parse_Integer__value()
        {
            CreateTextFile("{ \"IntValue\": 123 }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "IntValue", new RawSettings("123") }
                        }));
        }

        [Test]
        public void Should_parse_Double_value()
        {
            CreateTextFile("{ \"DoubleValue\": 123.321 }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "DoubleValue", new RawSettings("123,321") }
                        }));
        }

        [Test]
        public void Should_parse_Boolean_value()
        {
            CreateTextFile("{ \"BooleanValue\": true }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "BooleanValue", new RawSettings("True") }
                        }));
        }

        [Test]
        public void Should_parse_Null_value()
        {
            CreateTextFile("{ \"NullValue\": null }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "NullValue", new RawSettings(null) }
                        }));
        }

        [Test]
        public void Should_parse_Array_value()
        {
            CreateTextFile("{ \"IntArray\": [1, 2, 3] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
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
            CreateTextFile("{ \"Object\": { \"StringValue\": \"str\" } }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
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
            CreateTextFile("{ \"Array\": [{ \"StringValue\": \"str\" }, { \"IntValue\": 123 }] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
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
            CreateTextFile("{ \"Array\": [null, null] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
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
            CreateTextFile("{ \"Array\": [[\"s\", \"t\"], [\"r\"]] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
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
        public void Should_Observe_file()
        {
            CreateTextFile("{ \"Param1\": \"set1\" }");

            var val = 0;
            var jcs = new JsonFileSource(TestFileName);
            jcs.Observe().Subscribe(settings =>
            {
                val++;
                settings.Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            {"Param2", new RawSettings("set2")}
                        }));
            });

            CreateTextFile("{ \"Param2\": \"set2\" }");

            jcs.Observe().Subscribe(settings =>
            {
                val++;
                settings.Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            {"Param2", new RawSettings("set2")}
                        }));
            });

            // CR(krait): It's not a very good idea to add such long pauses in tests. When there are a plenty of such tests running them becomes painful.
            // CR(krait): Also, it's not good to rely on exact delays: if a test agent is very slow and the file watching thread gets stuck, or whatever else could happen, the test might fail. The more reliable way is to check for a condition in a loop with timeout. See: https://git.skbkontur.ru/ke/core-infra-testing/blob/master/Kontur.Core.Infra.Testing/AssertionAssertions.cs
            Thread.Sleep(TimeSpan.FromSeconds(11));
            val.Should().Be(2);
        }
    }
}