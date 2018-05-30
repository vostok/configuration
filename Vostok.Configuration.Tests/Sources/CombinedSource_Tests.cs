using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture, SingleThreaded]
    public class CombinedSource_Tests
    {
        private const string TestFile1Name = "test1_CombinedSource.json";
        private const string TestFile2Name = "test2_CombinedSource.json";
        private const string TestFile3Name = "test3_CombinedSource.json";
        private const string TestFile4Name = "test4_CombinedSource.json";

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFile1Name);
            File.Delete(TestFile2Name);
            File.Delete(TestFile3Name);
            File.Delete(TestFile4Name);
        }

        private static void CreateTextFile(int n, string text)
        {
            var fileName = string.Empty;
            switch (n)
            {
                case 1: fileName = TestFile1Name;   break;
                case 2: fileName = TestFile2Name;   break;
                case 3: fileName = TestFile3Name;   break;
                case 4: fileName = TestFile4Name;   break;
            }
            using (var file = new StreamWriter(fileName, false))
                file.WriteLine(text);
        }

        private static CombinedSource CreateCombinedSource(int cnt, SourceCombineOptions sourceCombineOptions = SourceCombineOptions.LastIsMain, CombineOptions combineOptions = CombineOptions.Override)
        {
            var list = new IConfigurationSource[]
            {
                new JsonFileSource(TestFile1Name),
                new JsonFileSource(TestFile2Name),
                new JsonFileSource(TestFile3Name),
                new JsonFileSource(TestFile4Name),
            };
            if (cnt > 0 && cnt <= 4)
                return new CombinedSource(list.Take(cnt).ToArray(), sourceCombineOptions, combineOptions);
            return new CombinedSource();
        }

        [Test]
        public void Should_return_null_if_no_sources()
        {
            using (var cs = CreateCombinedSource(0))
                cs.Get().Should().BeNull();
        }

        [Test]
        public void Should_merge_sources_with_override_values()
        {
            CreateTextFile(1, "{ \"value 1\": \"string 1\" }");
            CreateTextFile(2, "{ \"value 2\": \"string 2\" }");
            CreateTextFile(3, "{ \"value 2\": \"string 22\" }");

            using (var cs = CreateCombinedSource(3, SourceCombineOptions.FirstIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"].Value.Should().Be("string 1");
                result["value 2"].Value.Should().Be("string 2");
                result.Children.First().Value.Should().Be("string 1");
                result.Children.Last().Value.Should().Be("string 2");
            }
            using (var cs = CreateCombinedSource(3, SourceCombineOptions.LastIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"].Value.Should().Be("string 1");
                result["value 2"].Value.Should().Be("string 22");
                result.Children.First().Value.Should().Be("string 1");
                result.Children.Last().Value.Should().Be("string 22");
            }
        }

        [Test]
        public void Should_merge_sources_with_override_objects()
        {
            CreateTextFile(1, "{ \"value 1\": { \"subval 1\": \"string 1\" } }");
            CreateTextFile(2, "{ \"value 2\": { \"subval 1\": \"string 2\" } }");
            CreateTextFile(3, "{ \"value 2\": { \"subval 2\": \"string 22\" } }");

            using (var cs = CreateCombinedSource(3, SourceCombineOptions.FirstIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"]["subval 1"].Value.Should().Be("string 1");
                result["value 2"]["subval 1"].Value.Should().Be("string 2");
            }
            using (var cs = CreateCombinedSource(3, SourceCombineOptions.LastIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"]["subval 1"].Value.Should().Be("string 1");
                result["value 2"]["subval 2"].Value.Should().Be("string 22");
            }
        }

        [Test]
        public void Should_merge_sources_with_deep_merge_objects()
        {
            CreateTextFile(1, "{ \"value 1\": { \"subval 1\": \"string 1\" } }");
            CreateTextFile(2, "{ \"value 1\": { \"subval 1\": \"string 11\" } }");
            CreateTextFile(3, "{ \"value 2\": { \"subval 1\": \"string 2\" } }");
            CreateTextFile(4, "{ \"value 2\": { \"subval 2\": \"string 22\" } }");

            using (var cs = CreateCombinedSource(4, SourceCombineOptions.FirstIsMain, CombineOptions.DeepMerge))
            {
                var result = cs.Get();
                result["value 1"]["subval 1"].Value.Should().Be("string 1");
                result["value 2"]["subval 1"].Value.Should().Be("string 2");
                result["value 2"]["subval 2"].Value.Should().Be("string 22");
            }
            using (var cs = CreateCombinedSource(4, SourceCombineOptions.LastIsMain, CombineOptions.DeepMerge))
            {
                var result = cs.Get();
                result["value 1"]["subval 1"].Value.Should().Be("string 11");
                result["value 2"]["subval 1"].Value.Should().Be("string 2");
                result["value 2"]["subval 2"].Value.Should().Be("string 22");
            }
        }

        [Test]
        public void Should_merge_sources_with_override_arrays()
        {
            CreateTextFile(1, "{ \"value 1\": [ \"1\", \"11\" ] }");
            CreateTextFile(2, "{ \"value 2\": [ \"2\", \"22\" ] }");
            CreateTextFile(3, "{ \"value 2\": [ \"3\", \"33\" ] }");

            using (var cs = CreateCombinedSource(3, SourceCombineOptions.FirstIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"].Children.First().Value.Should().Be("1");
                result["value 1"].Children.Last().Value.Should().Be("11");
                result["value 2"].Children.First().Value.Should().Be("2");
                result["value 2"].Children.Last().Value.Should().Be("22");
            }
            using (var cs = CreateCombinedSource(3, SourceCombineOptions.LastIsMain, CombineOptions.Override))
            {
                var result = cs.Get();
                result["value 1"].Children.First().Value.Should().Be("1");
                result["value 1"].Children.Last().Value.Should().Be("11");
                result["value 2"].Children.First().Value.Should().Be("3");
                result["value 2"].Children.Last().Value.Should().Be("33");
            }
        }

        [Test]
        public void Should_merge_sources_with_deep_merge_arrays()
        {
            CreateTextFile(1, "{ \"value\": [ \"1\", \"2\" ] }");
            CreateTextFile(2, "{ \"value\": [ \"3\", \"2\", \"5\" ] }");
            CreateTextFile(3, "{ \"value\": [ \"3\", \"4\" ] }");

            using (var cs = CreateCombinedSource(3, SourceCombineOptions.FirstIsMain, CombineOptions.DeepMerge))
            {
                var result = cs.Get();
                result["value"].Children.ElementAt(0).Value.Should().Be("1");
                result["value"].Children.ElementAt(1).Value.Should().Be("2");
                result["value"].Children.ElementAt(2).Value.Should().Be("3");
                result["value"].Children.ElementAt(3).Value.Should().Be("4");
//                result["value 2"]["subval 1"].Value.Should().Be("string 2");
//                result["value 2"]["subval 2"].Value.Should().Be("string 22");
            }
            using (var cs = CreateCombinedSource(3, SourceCombineOptions.LastIsMain, CombineOptions.DeepMerge))
            {
                var result = cs.Get();
                result["value 1"]["subval 1"].Value.Should().Be("string 11");
                result["value 2"]["subval 1"].Value.Should().Be("string 2");
                result["value 2"]["subval 2"].Value.Should().Be("string 22");
            }
        }

        /*[Test]
        public void Should_merge_simple_lists_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [1,2,3] }");
            CreateTextFile(2, "{ \"value\": [4,5] }");
            CreateTextFile(3, "{ \"value\": [1,2] }");

            using (var cs = CreateCombinedSource(3, ListCombineOptions.FirstOnly))
                cs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings("1", "0"),
                                    ["1"] = new RawSettings("2", "1"),
                                    ["2"] = new RawSettings("3", "2"),
                                }, "value")
                            }
                        }, "root"));
        }*/

        /*[Test]
        public void Should_merge_simple_lists_UnionDist()
        {
            CreateTextFile(1, "{ \"value\": [1,2,3] }");
            CreateTextFile(2, "{ \"value\": [4,5] }");
            CreateTextFile(3, "{ \"value\": [1,2] }");

            using (var cs = CreateCombinedSource(3, ListCombineOptions.UnionAll))
                cs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings("1", "0"),
                                    ["1"] = new RawSettings("2", "1"),
                                    ["2"] = new RawSettings("3", "2"),
                                    ["3"] = new RawSettings("4", "3"),
                                    ["4"] = new RawSettings("5", "4"),
                                    ["5"] = new RawSettings("1", "5"),
                                    ["6"] = new RawSettings("2", "6"),
                                }, "value")
                            }
                        }, "root"));
        }*/

        /*[Test]
        public void Should_merge_dictionaries_of_objects()
        {
            CreateTextFile(1, "{ \"value\": { \"ObjValue\": 1, \"ObjArray\": [1,2] } }");
            CreateTextFile(2, "{ \"value\": { \"ObjValue\": 2, \"ObjArray\": [3,4] } }");
            CreateTextFile(3, "{ \"value\": { \"ObjValue 2\": 3, \"ObjArray\": [5,6] } }");

            using (var cs = CreateCombinedSource(3, ListCombineOptions.UnionAll))
            {
                var result = cs.Get();
                result.Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    { "ObjValue", new RawSettings("1", "ObjValue") },
                                    { "ObjValue 2", new RawSettings("3", "ObjValue 2") },
                                    { "ObjArray", new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("1", "0"),
                                            ["1"] = new RawSettings("2", "1"),
                                            ["2"] = new RawSettings("3", "2"),
                                            ["3"] = new RawSettings("4", "3"),
                                            ["4"] = new RawSettings("5", "4"),
                                            ["5"] = new RawSettings("6", "5"),
                                        }, "ObjArray")
                                    },
                                }, "value") },
                        }, "root"));}
        }*/

        /*[Test]
        public void Should_merge_lists_of_objects_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [ { \"Obj_1_Value\": 1 }, { \"Obj_2_Value\": 1 } ] }");
            CreateTextFile(2, "{ \"value\": [ { \"Obj_1_Value\": 2 }, { \"Obj_2_Value\": 2 } ] }");

            using (var cs = CreateCombinedSource(2, ListCombineOptions.FirstOnly))
            {
                var result = cs.Get();
                result.Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_1_Value", new RawSettings("1", "Obj_1_Value") },
                                        }, "0"),
                                    ["1"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_2_Value", new RawSettings("1", "Obj_2_Value") },
                                        }, "1"),
                                }, "value")
                            }
                        }, "root"));}
        }*/

        /*[Test]
        public void Should_merge_lists_of_objects_UnionDist()
        {
            CreateTextFile(1, "{ \"value\": [ { \"Obj_1_Value\": 1 }, { \"Obj_2_Value\": 1 } ] }");
            CreateTextFile(2, "{ \"value\": [ { \"Obj_1_Value\": 2 }, { \"Obj_2_Value\": 2 } ] }");

            using (var cs = CreateCombinedSource(2, ListCombineOptions.UnionAll))
                cs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_1_Value", new RawSettings("1", "Obj_1_Value") },
                                        }, "0"),
                                    ["1"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_2_Value", new RawSettings("1", "Obj_2_Value") },
                                        }, "1"),
                                    ["2"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_1_Value", new RawSettings("2", "Obj_1_Value") },
                                        }, "2"),
                                    ["3"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            { "Obj_2_Value", new RawSettings("2", "Obj_2_Value") },
                                        }, "3"),
                                }, "value")
                            }
                        }, "root"));
        }*/

        /*[Test]
        public void Should_merge_lists_of_lists_FirstOnly()
        {
            CreateTextFile(1, "{ \"value\": [ [1,2], [3,4] ] }");
            CreateTextFile(2, "{ \"value\": [ [5,6], [1,2] ] }");

            using (var cs = CreateCombinedSource(2, ListCombineOptions.FirstOnly))
                cs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("1", "0"),
                                            ["1"] = new RawSettings("2", "1"),
                                        }),
                                    ["1"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("3", "0"),
                                            ["1"] = new RawSettings("4", "1"),
                                        }),
                                }, "value")
                            }
                        }, "root"));
        }*/

        /*[Test]
        public void Should_merge_lists_of_lists_UnionAll()
        {
            CreateTextFile(1, "{ \"value\": [ [1,2], [3,4] ] }");
            CreateTextFile(2, "{ \"value\": [ [5,6], [1,2] ] }");

            using (var cs = CreateCombinedSource(2, ListCombineOptions.UnionAll))
                cs.Get().Should().BeEquivalentTo(
                    new RawSettings(
                        new OrderedDictionary
                        {
                            { "value", new RawSettings(
                                new OrderedDictionary
                                {
                                    ["0"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("1", "0"),
                                            ["1"] = new RawSettings("2", "1"),
                                        }, "0"),
                                    ["1"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("3", "0"),
                                            ["1"] = new RawSettings("4", "1"),
                                        }, "1"),
                                    ["2"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("5", "0"),
                                            ["1"] = new RawSettings("6", "1"),
                                        }, "2"),
                                    ["3"] = new RawSettings(
                                        new OrderedDictionary
                                        {
                                            ["0"] = new RawSettings("1", "0"),
                                            ["1"] = new RawSettings("2", "1"),
                                        }, "3"),
                                }, "value")
                            }
                        }, "root"));
        }*/

        /*[Test, Explicit("Not stable on mass tests")]
        public void Should_observe_file()
        {
            new Action(() => ShouldObserveFileTest_ReturnsCountOfReceives().Should().Be(2)).ShouldPassIn(1.Seconds());
        }
        private int ShouldObserveFileTest_ReturnsCountOfReceives()
        {
            CreateTextFile(1, "{ \"value 1\": 1, \"list\": [1,2] }");
            CreateTextFile(2, "{ \"value 2\": 2 }");
            var val = 0;

            using (var ccs = CreateCombinedSource(2, ListCombineOptions.FirstOnly))
            {
                var sub = ccs.Observe().Subscribe(settings =>
                {
                    val++;
                    settings.Should().BeEquivalentTo(
                        new RawSettings(
                            new OrderedDictionary
                            {
                                { "value 1", new RawSettings("1", "value 1") },
                                { "value 2", new RawSettings("2", "value 2") },
                                { "list", new RawSettings(
                                    new OrderedDictionary
                                    {
                                        ["0"] = new RawSettings("1", "0"),
                                        ["1"] = new RawSettings("2", "1"),
                                    }, "list") },
                            }));
                });

                Thread.Sleep(200.Milliseconds());
                CreateTextFile(2, "{ \"value 2\": 2, \"list\": [3,4] }");
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();
            }
            return val;
        }*/
    }
}