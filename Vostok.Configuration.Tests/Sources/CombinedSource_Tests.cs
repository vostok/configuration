using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Convertions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class CombinedSource_Tests
    {
        private const string TestFile1Name = "test1.json";
        private const string TestFile2Name = "test2.json";
        private const string TestFile3Name = "test3.json";

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFile1Name);
            File.Delete(TestFile2Name);
            File.Delete(TestFile3Name);
        }

        private static void CreateTextFile(int n, string text)
        {
            var fileName = string.Empty;
            switch (n)
            {
                case 1:
                    fileName = TestFile1Name;
                    break;
                case 2:
                    fileName = TestFile2Name;
                    break;
                case 3:
                    fileName = TestFile3Name;
                    break;
            }
            using (var file = new StreamWriter(fileName, false))
                file.WriteLine(text);
        }

        private static CombinedSource CreateCombinedSource(int cnt, ListCombineOptions listCombineOptions = ListCombineOptions.FirstOnly)
        {
            var time = 300;
            switch (cnt)
            {
                case 1:
                    return new CombinedSource(
                        new IConfigurationSource[]{ new JsonFileSource(TestFile1Name, time) },
                        listCombineOptions);
                case 2:
                    return new CombinedSource(
                        new IConfigurationSource[] { new JsonFileSource(TestFile1Name, time), new JsonFileSource(TestFile2Name, time) },
                        listCombineOptions);
                case 3:
                    return new CombinedSource(
                        new IConfigurationSource[] { new JsonFileSource(TestFile1Name, time), new JsonFileSource(TestFile2Name, time), new JsonFileSource(TestFile3Name, time) },
                        listCombineOptions);
                default:
                    return new CombinedSource();
            }
        }

        [Test]
        public void Should_return_null_if_no_sources()
        {
            CreateCombinedSource(0).Get().Should().BeNull();
        }

        [Test]
        public void Should_merge_simple_dictionaries()
        {
            CreateTextFile(1, "{ \"value 1\": \"string 1\" }");
            CreateTextFile(2, "{ \"value 2\": \"string 2\" }");
            CreateTextFile(3, "{ \"value 2\": \"string 22\" }");

            CreateCombinedSource(3).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value 1", new RawSettings("string 1") },
                            { "value 2", new RawSettings("string 2") },
                        }));
        }

        [Test]
        public void Should_merge_simple_lists_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [1,2,3] }");
            CreateTextFile(2, "{ \"value\": [4,5] }");
            CreateTextFile(3, "{ \"value\": [1,2] }");

            CreateCombinedSource(3, ListCombineOptions.FirstOnly).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings("1"),
                                    new RawSettings("2"),
                                    new RawSettings("3"),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_merge_simple_lists_UnionDist()
        {
            CreateTextFile(1, "{ \"value\": [1,2,3] }");
            CreateTextFile(2, "{ \"value\": [4,5] }");
            CreateTextFile(3, "{ \"value\": [1,2] }");

            CreateCombinedSource(3, ListCombineOptions.UnionAll).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings("1"),
                                    new RawSettings("2"),
                                    new RawSettings("3"),
                                    new RawSettings("4"),
                                    new RawSettings("5"),
                                    new RawSettings("1"),
                                    new RawSettings("2"),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_merge_dictionaries_of_objects()
        {
            CreateTextFile(1, "{ \"value\": { \"ObjValue\": 1, \"ObjArray\": [1,2] } }");
            CreateTextFile(2, "{ \"value\": { \"ObjValue\": 2, \"ObjArray\": [3,4] } }");
            CreateTextFile(3, "{ \"value\": { \"ObjValue 2\": 3, \"ObjArray\": [5,6] } }");

            CreateCombinedSource(3, ListCombineOptions.UnionAll).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "ObjValue", new RawSettings("1") },
                                    { "ObjValue 2", new RawSettings("3") },
                                    { "ObjArray", new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("1"),
                                            new RawSettings("2"),
                                            new RawSettings("3"),
                                            new RawSettings("4"),
                                            new RawSettings("5"),
                                            new RawSettings("6"),
                                        })
                                    },
                                }) },
                        }));
        }

        [Test]
        public void Should_merge_lists_of_objects_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [ { \"Obj_1_Value\": 1 }, { \"Obj_2_Value\": 1 } ] }");
            CreateTextFile(2, "{ \"value\": [ { \"Obj_1_Value\": 2 }, { \"Obj_2_Value\": 2 } ] }");

            CreateCombinedSource(2).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_1_Value", new RawSettings("1") },
                                        }),
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_2_Value", new RawSettings("1") },
                                        }),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_merge_lists_of_objects_UnionDist()
        {
            CreateTextFile(1, "{ \"value\": [ { \"Obj_1_Value\": 1 }, { \"Obj_2_Value\": 1 } ] }");
            CreateTextFile(2, "{ \"value\": [ { \"Obj_1_Value\": 2 }, { \"Obj_2_Value\": 2 } ] }");

            CreateCombinedSource(2, ListCombineOptions.UnionAll).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_1_Value", new RawSettings("1") },
                                        }),
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_2_Value", new RawSettings("1") },
                                        }),
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_1_Value", new RawSettings("2") },
                                        }),
                                    new RawSettings(
                                        new Dictionary<string, RawSettings>
                                        {
                                            { "Obj_2_Value", new RawSettings("2") },
                                        }),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_merge_lists_of_lists_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [ [1,2], [3,4] ] }");
            CreateTextFile(2, "{ \"value\": [ [5,6], [1,2] ] }");

            CreateCombinedSource(2, ListCombineOptions.FirstOnly).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("1"),
                                            new RawSettings("2"),
                                        }),
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("3"),
                                            new RawSettings("4"),
                                        }),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_merge_lists_of_lists_UnionAll()
        {
            CreateTextFile(1, "{ \"value\": [ [1,2], [3,4] ] }");
            CreateTextFile(2, "{ \"value\": [ [5,6], [1,2] ] }");

            CreateCombinedSource(2, ListCombineOptions.UnionAll).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "value", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("1"),
                                            new RawSettings("2"),
                                        }),
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("3"),
                                            new RawSettings("4"),
                                        }),
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("5"),
                                            new RawSettings("6"),
                                        }),
                                    new RawSettings(
                                        new List<RawSettings>
                                        {
                                            new RawSettings("1"),
                                            new RawSettings("2"),
                                        }),
                                })
                            }
                        }));
        }

        [Test]
        public void Should_observe_file()
        {
            new Action(() => Should_observe_file_test().Should().Be(1)).ShouldPassIn(3.Seconds());
        }
        private int Should_observe_file_test()
        {
            CreateTextFile(1, "{ \"value 1\": 1, \"list\": [1,2] }");
            CreateTextFile(2, "{ \"value 2\": 2 }");
            var val = 0;

            var ccs = CreateCombinedSource(2, ListCombineOptions.FirstOnly);
            ccs.Observe().Subscribe(
                settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new Dictionary<string, RawSettings>
                            {
                                { "value 1", new RawSettings("1") },
                                { "value 2", new RawSettings("2") },
                                { "list", new RawSettings(
                                    new List<RawSettings>
                                    {
                                        new RawSettings("1"),
                                        new RawSettings("2"),
                                    }) },
                            }));
                });

            Thread.Sleep(1.Seconds());
            CreateTextFile(2, "{ \"value 2\": 2, \"list\": [3,4] }");
            Thread.Sleep(1.Seconds());

            return val;
        }
    }
}