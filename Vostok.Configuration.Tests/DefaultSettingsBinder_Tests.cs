using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons;
using Vostok.Commons.Parsers;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.SettingsTree;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
#pragma warning disable 414
#pragma warning disable 169
#pragma warning disable 649

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;

        [SetUp]
        public void SetUp()
        {
            binder = new DefaultSettingsBinder().WithDefaultParsers();
        }

        [TestCase("FaLsE", false, TestName = "BoolValue")]
        [TestCase("255", (byte) 255, TestName = "ByteValue")]
        [TestCase("q", 'q', TestName = "CharValue")]
        [TestCase("123.456", 123.456d, TestName = "DoubleValue")]
        [TestCase("123,456", 123.456f, TestName = "FloatValue")]
        [TestCase("12345", 12345, TestName = "IntValue")]
        [TestCase("123456787654321", 123456787654321L, TestName = "LongValue")]
        [TestCase("-1", (sbyte) -1, TestName = "SbyteValue")]
        [TestCase("100", (short) 100, TestName = "ShortValue")]
        [TestCase("1234567", (uint) 1234567, TestName = "UintValue")]
        [TestCase("123456787654321", (ulong) 123456787654321L, TestName = "UlongValue")]
        [TestCase("200", (ushort) 200, TestName = "UshortValue")]
        public void Should_bind_to_Primitive<T>(string value, T res)
        {
            var settings = new ValueNode(value);
            binder.Bind<T>(settings).Should().Be(res);
        }

        [Test]
        public void Should_bind_to_Primitive_from_single_children_value()
        {
            var sets1 = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "key", new ValueNode("123") }
            });
            binder.Bind<int>(sets1).Should().Be(123);

            var sets2 = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("321"),
            });
            binder.Bind<int>(sets2).Should().Be(321);
        }

        [Test]
        public void Should_bind_to_Decimal()
        {
            var settings = new ValueNode("123,456");
            binder.Bind<decimal>(settings).Should().Be(123.456m);
        }

        [Test]
        public void Should_bind_to_TimeSpan()
        {
            var settings = new ValueNode("1 second");
            binder.Bind<TimeSpan>(settings).Should().Be(new TimeSpan(0, 0, 0, 1, 0));
        }

        [Test]
        public void Should_bind_to_IPAddress()
        {
            var settings = new ValueNode("192.168.1.10");
            binder.Bind<IPAddress>(settings).Should().Be(
                new IPAddress(new byte[] {192, 168, 1, 10}));

            settings = new ValueNode("2001:0db8:11a3:09d7:1f34:8a2e:07a0:765d");
            binder.Bind<IPAddress>(settings).Should().Be(
                new IPAddress(new byte[] {32,1,  13,184,  17,163,  9,215,  31,52,  138,46,  7,160,  118,93}));
        }

        [Test]
        public void Should_bind_to_IPEndPoint()
        {
            var settings = new ValueNode("192.168.1.10:80");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(new byte[] {192, 168, 1, 10}), 80));
        }

        [Test]
        public void Should_bind_to_DataRate()
        {
            var settings = new ValueNode("10/sec");
            binder.Bind<DataRate>(settings).Should().Be(
                DataRate.FromBytesPerSecond(10));
        }

        [Test]
        public void Should_bind_to_DataSize()
        {
            var settings = new ValueNode("10 bytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromBytes(10));
        }

        [Test]
        public void Should_bind_to_Guid()
        {
            const string guid = "936DA01F-9ABD-4d9d-80C7-02AF85C822A8";
            var settings = new ValueNode(guid);
            binder.Bind<Guid>(settings).Should().Be(new Guid(guid));
        }

        [Test]
        public void Should_bind_to_Uri()
        {
            var settings = new ValueNode("http://example.com");
            binder.Bind<Uri>(settings).Should().Be(new Uri("http://example.com", UriKind.Absolute));
        }

        [Test]
        public void Should_bind_to_String()
        {
            var settings = new ValueNode("somestring");
            binder.Bind<string>(settings).Should().Be("somestring");
        }

        [Test]
        public void Should_bind_with_custom_parser_over_ITypeParser()
        {
            var settings = new ValueNode("some,string");
            var result = binder.WithCustomParser<CST>(new CommaSeparatedTextParser())
                .Bind<CST>(settings);
            result.Should().BeEquivalentTo(
                new CST{ Strings = new [] {"some", "string"} });
        }

        [Test]
        public void Should_bind_with_custom_parser_over_delegate()
        {
            var settings = new ValueNode("some;string");
            var result = binder.WithCustomParser<SST>(TryParseSemicolonSeparatedText)
                .Bind<SST>(settings);
            result.Should().BeEquivalentTo(
                new SST{ Strings = new [] {"some", "string"} });
        }

        [Test]
        public void Should_bind_to_NullableInt()
        {
            var settings = new ValueNode("10");
            binder.Bind<int?>(settings).Should().Be(10);

            settings = new ValueNode(null, "");
            binder.Bind<int?>(settings).Should().Be(null);
        }

        [Test]
        public void Should_bind_to_Enum_by_value_or_name()
        {
            var settings = new ValueNode("10");
            binder.Bind<ConsoleColor>(settings).Should().Be(ConsoleColor.Green);

            settings = new ValueNode("grEEn");
            binder.Bind<ConsoleColor>(settings).Should().Be(ConsoleColor.Green);
        }

        [Test]
        public void Should_bind_to_DateTime()
        {
            var settings = new ValueNode("2018-03-14T15:09:26.535");
            binder.Bind<DateTime>(settings).Should().Be(new DateTime(2018, 3, 14, 15, 9, 26, 535).ToUniversalTime());
        }

        [Test]
        public void Should_bind_to_DateTimeOffset()
        {
            var settings = new ValueNode("2018-03-14T15:09:26.535");
            binder.Bind<DateTimeOffset>(settings).Should().Be(new DateTimeOffset(2018, 3, 14, 15, 9, 26, 535, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)));
        }

        private struct Struct1
        {
            public int IntValue;
            public string StringValue;
        }
        private struct Struct2
        {
            public string StringValue;
            public Struct1 Struct1 { get; set; }
            public int IgnoredRoIntProp { get; }
        }

        [Test]
        public void Should_bind_to_Struct()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "IntValue", new ValueNode("10") },
                { "StringValue", new ValueNode("str") },
            });
            binder.Bind<Struct1>(settings)
                .Should().BeEquivalentTo(
                    new Struct1{ IntValue = 10, StringValue = "str" }
                );
        }
        
        [RequiredByDefault]
        private struct MyRequiredStruct
        {
            public int Required { get; set; }
            [Optional]
            public int Optional { get; set; }
            [Optional]
            public int? OptionalNullable { get; set; }
        }

        private struct MyOptionalStruct
        {
            [Required]
            public int Required { get; set; }
            [Required]
            public int? RequiredNullable { get; set; }
            public int Optional { get; set; }
        }

        [Test]
        public void Should_bind_to_Struct_with_checking_attributes()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "Required", new ValueNode("1") },
                //{ "Optional", new ValueNode("2") },
                { "OptionalNullable", new ValueNode(null, "") },
            });
            binder.Bind<MyRequiredStruct>(settings)
                .Should().BeEquivalentTo(
                    new MyRequiredStruct
                    {
                        Required = 1,
                        Optional = default,
                        OptionalNullable = null,
                    }
                );

            settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "Required", new ValueNode("1") },
                { "RequiredNullable", new ValueNode("0") },
                //{ "Optional", new ValueNode("2") },
            });
            binder.Bind<MyOptionalStruct>(settings)
                .Should().BeEquivalentTo(
                    new MyOptionalStruct
                    {
                        Required = 1,
                        RequiredNullable = 0,
                        Optional = default,
                    }
                );
        }

        [Test]
        public void Should_bind_to_StructWithStruct()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "StringValue", new ValueNode("strstr") },
                { "IgnoredRoIntProp", new ValueNode("123") },
                { "Struct1", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "IntValue", new ValueNode("10") },
                        { "StringValue", new ValueNode("str") },
                    })
                }
            });
            binder.Bind<Struct2>(settings)
                .Should().BeEquivalentTo(
                    new Struct2
                    {
                        StringValue = "strstr",
                        Struct1 = new Struct1
                        {
                            IntValue = 10,
                            StringValue = "str",
                        }
                    }
                );
        }

        [Test]
        public void Should_bind_to_HashSet()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("val_1"),
                new ValueNode("val_2"),
                new ValueNode("val_3"),
            });
            binder.Bind<HashSet<string>>(settings)
                .Should().BeEquivalentTo(new HashSet<string> { "val_1", "val_2", "val_3" });
        }

        private class MyClass
        {
            private int privateIntField;
            private string PrivateStrGetProp { get; }
            public double PublicDoubleSetProp { get; set; }
            public readonly int PublicIntReadonlyField;
            private const string PrivateConstStringField = "qwe";
            public static string PublicStringStaticField;
            public static int PublicIntStaticProp { get; set; }
            public Struct1 Struct1 { get; set; }
            public MyClass2 Class2 { get; set; }
            public MyClass2 Class2Null { get; set; }
            public int[] PublicIntArrayProp { get; set; }
            public int[] PublicIntArrayPropNull { get; set; }
            public List<string> PublicStringListProp { get; set; }
            public List<string> PublicStringListPropNull { get; set; }
            public Dictionary<int, string> PublicDictionaryProp { get; set; }
            public Dictionary<int, string> PublicDictionaryPropNull { get; set; }
            public double? PublicNullableDoubleSetProp { get; set; }

            public MyClass() {}
            public MyClass(bool testOnly)
            {
                PublicIntReadonlyField = 20;
                PublicStringStaticField = "statStr";
                PublicIntStaticProp = 1234;
            }

            public int GetPublicIntReadonlyProp => PublicIntReadonlyField;
        }
        private class MyClass2
        {
            public int PublicIntProp { get; set; }
        }
        private class MyClass3
        {
            public int PublicIntField;
        }

        [Test]
        public void Should_bind_to_Class()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "privateIntField", new ValueNode("10") },
                { "PrivateStrGetProp", new ValueNode("str") },
                { "PublicDoubleSetProp", new ValueNode("1.23") },
                { "PublicIntReadonlyField", new ValueNode("20") },
                { "PrivateConstStringField", new ValueNode("ewq") },
                { "PublicStringStaticField", new ValueNode("statStr") },
                { "PublicIntStaticProp", new ValueNode("1234") },
                { "PublicNullableDoubleSetProp", new ValueNode(null, "") },
                { "PublicIntArrayProp", new  ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("1"),
                    new ValueNode("2"),
                }) },
                { "PublicIntArrayPropNull", new ValueNode(null, "") },
                { "PublicStringListProp", new ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("str1"),
                    new ValueNode("str2"),
                }) },
                { "PublicStringListPropNull", new ValueNode(null, "") },
                { "PublicDictionaryProp", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "1", new ValueNode("str1", "1") },
                    { "2", new ValueNode("str2", "2") },
                }) },
                { "PublicDictionaryPropNull", new ValueNode(null, "") },
                { "Struct1", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "IntValue", new ValueNode("10") },
                        { "StringValue", new ValueNode("structString") },
                    })
                },
                { "Class2", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "PublicIntProp", new ValueNode("111") },
                    })
                },
                { "Class2Null", new ValueNode(null, "") },

                { "GetPublicIntReadonlyProp", new ValueNode("321") },
            });
            binder.Bind<MyClass>(settings)
                .Should().BeEquivalentTo(
                    new MyClass(true)
                    {
                        PublicDoubleSetProp = 1.23,
                        Struct1 = new Struct1
                        {
                            IntValue = 10,
                            StringValue = "structString",
                        },
                        Class2 = new MyClass2
                        {
                            PublicIntProp = 111,
                        },
                        Class2Null = null,
                        PublicIntArrayProp = new [] { 1, 2 },
                        PublicIntArrayPropNull = null,
                        PublicStringListProp = new List<string> { "str1", "str2" },
                        PublicStringListPropNull = null,
                        PublicDictionaryProp = new Dictionary<int, string> { {1, "str1"}, {2, "str2"} },
                        PublicDictionaryPropNull = null,
                        PublicNullableDoubleSetProp = null,
                    }
                );
        }

        [RequiredByDefault]
        private class MyRequiredClass
        {
            public int Required { get; set; }
            [Optional]
            public int Optional { get; set; }
            [Optional]
            public int? OptionalNullable { get; set; }
        }

        private class MyOptionalClass
        {
            [Required]
            public int Required { get; set; }
            [Required]
            public int? RequiredNullable { get; set; }
            public int Optional { get; set; }
        }

        [Test]
        public void Should_bind_to_class_with_checking_attributes()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "Required", new ValueNode("1") },
                //{ "Optional", new ValueNode("2") },
                { "OptionalNullable", new ValueNode(null, "") },
            });
            binder.Bind<MyRequiredClass>(settings)
                .Should().BeEquivalentTo(
                    new MyRequiredClass
                    {
                        Required = 1,
                        Optional = default,
                        OptionalNullable = null,
                    }
                );

            settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "Required", new ValueNode("1") },
                { "RequiredNullable", new ValueNode("0") },
                //{ "Optional", new ValueNode("2") },
            });
            binder.Bind<MyOptionalClass>(settings)
                .Should().BeEquivalentTo(
                    new MyOptionalClass
                    {
                        Required = 1,
                        RequiredNullable = 0,
                        Optional = default,
                    }
                );
        }

        [Test]
        public void Should_bind_to_IntArray()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            binder.Bind<int[]>(settings).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ArrayOfIntArrays()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("1"),
                    new ValueNode("2"),
                }),
                new ArrayNode(new List<ISettingsNode>
                {
                    new ValueNode("3"),
                    new ValueNode("4"),
                }),
            });
            binder.Bind<int[][]>(settings).Should().BeEquivalentTo(new [] {1,2}, new[] {3,4});
        }

        [Test]
        public void Should_bind_to_ObjectArray()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "PublicIntProp", new ValueNode("1") },
                }),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "PublicIntProp", new ValueNode("2") },
                }),
            });
            binder.Bind<MyClass2[]>(settings)
                .Should().BeEquivalentTo(
                    new MyClass2 { PublicIntProp = 1, },
                    new MyClass2 { PublicIntProp = 2, });
        }

        [Test]
        public void Should_bind_to_IntList()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            binder.Bind<List<int>>(settings).Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ObjectList()
        {
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "PublicIntProp", new ValueNode("1") },
                }),
                new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "PublicIntProp", new ValueNode("2") },
                }),
            });
            binder.Bind<List<MyClass2>>(settings)
                .Should().BeEquivalentTo(
                    new List<MyClass2>
                    {
                        new MyClass2{ PublicIntProp = 1 },
                        new MyClass2{ PublicIntProp = 2 },
                    });
        }

        [Test]
        public void Should_bind_to_IntCollection()
        {
            ICollection<int> list = new List<int> { 1, 2 };
            var settings = new ArrayNode(new List<ISettingsNode>
            {
                new ValueNode("1"),
                new ValueNode("2"),
            });
            binder.Bind<List<int>>(settings).Should().BeEquivalentTo(list);
        }

        [Test]
        public void Should_bind_to_IntIntDictionary()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "1", new ValueNode("10", "1") },
                { "2", new ValueNode("20", "2") },
            });
            binder.Bind<Dictionary<int, int>>(settings)
                .Should().BeEquivalentTo(
                    new Dictionary<int, int> { {1, 10}, {2, 20} });
        }

        [Test]
        public void Should_bind_to_StringObjectDictionary()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "key1", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "PublicIntProp", new ValueNode("1", "PublicIntProp") },
                    }, "key1")
                },
                { "key2", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "PublicIntProp", new ValueNode("2", "PublicIntProp") },
                    }, "key2")
                },
            });
            binder.Bind<Dictionary<string, MyClass2>>(settings)
                .Should().BeEquivalentTo(
                    new Dictionary<string, MyClass2>
                    {
                        {"key1", new MyClass2 {PublicIntProp = 1}},
                        {"key2", new MyClass2 {PublicIntProp = 2} }
                    });
        }

        [Test]
        public void Should_bind_to_GenericClass()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "PublicIntProp", new ValueNode("10") },
                { "PublicT1Prop", new ValueNode("str") },
                { "PublicT2Prop", new ValueNode("1.23") },
                { "PublicDictProp", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    { "key1", new ValueNode("1,2", "key1") },
                    { "key2", new ValueNode("2.3", "key2") },
                }) },
            });
            binder.Bind<GenericClass<string, double>>(settings)
                .Should().BeEquivalentTo(
                    new GenericClass<string, double>
                    {
                        PublicIntProp = 10,
                        PublicT1Prop = "str",
                        PublicT2Prop = 1.23,
                        PublicDictProp = new Dictionary<string, double>
                        {
                            { "key1", 1.2 },
                            { "key2", 2.3 },
                        }
                    }
                );
        }

        //-------Exceptions---------//

        [Test]
        [Order(0)]
        public void Should_throw_ArgumentNullException_on_unknown_data_type()
        {
            new Action(() => binder.Bind<CST>(new ValueNode("a,b,c"))).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Main()
        {
            new Action(() => binder.Bind<int>(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Dictionary()
        {
            new Action(() => binder.Bind<MyRequiredStruct>(new ValueNode(null, ""))).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Array()
        {
            new Action(() => binder.Bind<int[]>(new ValueNode(null, ""))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_List()
        {
            new Action(() => binder.Bind<List<int>>(new ValueNode(null, ""))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Class()
        {
            new Action(() => binder.Bind<MyRequiredClass>(new ValueNode(null, ""))).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Struct()
        {
            new Action(() => binder.Bind<MyRequiredStruct>(new ValueNode(null, ""))).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_InvalidCastException_Primitive_if_not_single_value_dictionary()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "key1", new ValueNode("123") },
                { "key2", new ValueNode("123") },
            });
            new Action(() => binder.Bind<int>(settings)).Should().Throw<ArgumentException>();
            
            settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>());
            new Action(() => binder.Bind<int>(settings)).Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_Primitive_if_wrong_type()
        {
            var settings = new ValueNode("str");
            new Action(() => binder.Bind<int>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_Enum_if_wrong_value()
        {
            var settings = new ValueNode("SeroBuroMalinovy");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();

            settings = new ValueNode("1_000_000");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_struct_required_field_or_prop_is_absent()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "WrongName", new ValueNode("10") }
            });
            new Action(() => binder.Bind<MyRequiredStruct>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_class_required_field_or_prop_is_absent()
        {
            var settings = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                { "WrongName", new ValueNode("10") }
            });
            new Action(() => binder.Bind<MyRequiredClass>(settings)).Should().Throw<InvalidCastException>();
        }

        private class CST
        {
            [Required]
            public string[] Strings { get; set; }
        }
        private class CommaSeparatedTextParser : ITypeParser
        {
            public bool TryParse(string s, out object value)
            {
                if (string.IsNullOrEmpty(s))
                {
                    value = null;
                    return false;
                }

                value = new CST { Strings = s.Split(',') };
                return true;
            }
        }

        private class SST
        {
            [Required]
            public string[] Strings { get; set; }
        }
        private bool TryParseSemicolonSeparatedText(string s, out SST value)
        {
            if (string.IsNullOrEmpty(s))
            {
                value = default(SST);
                return false;
            }

            value = new SST { Strings = s.Split(';') };
            return true;
        }

        private class GenericClass<T1, T2>
        {
            public int PublicIntProp { get; set; }
            public T1 PublicT1Prop { get; set; }
            public T2 PublicT2Prop { get; set; }
            public Dictionary<T1, T2> PublicDictProp { get; set; }
        }
    }
}