﻿using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons;
using Vostok.Commons.Parsers;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;

        internal class CST
        {
            public string[] Strings { get; set; }
        }
        internal class CommaSeparatedTextParser: ITypeParser
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

        internal class SST
        {
            public string[] Strings { get; set; }
        }
        private bool TryParseSemicolonSeparatedText(string s, out SST value)
        {
            if (string.IsNullOrEmpty(s))
            {
                value = default(SST);
                return false;
            }

            value = new SST {Strings = s.Split(';')};
            return true;
        }

        [SetUp]
        public void SetUp()
        {
            binder = new DefaultSettingsBinder();
        }

        [TestCase("FaLsE", false, TestName = "BoolValue")]
        [TestCase("255", (byte) 255, TestName = "ByteValue")]
        [TestCase("q", 'q', TestName = "CharValue")]
//        [TestCase("123.456", (decimal) 123456m, TestName = "DecimalValue")]  decimal is below
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
            var settings = new RawSettings(value);
            binder.Bind<T>(settings).Should().Be(res);
        }

        [Test]
        public void Should_bind_to_Decimal()
        {
            var settings = new RawSettings("123,456");
            binder.Bind<decimal>(settings).Should().Be(123.456m);
        }

        [Test]
        public void Should_bind_to_TimeSpan()
        {
            var settings = new RawSettings("1 second");
            binder.Bind<TimeSpan>(settings).Should().Be(new TimeSpan(0, 0, 0, 1, 0));
        }

        [Test]
        public void Should_bind_to_IPAddress()
        {
            var settings = new RawSettings("192.168.1.10");
            binder.Bind<IPAddress>(settings).Should().Be(
                new IPAddress(new byte[] {192, 168, 1, 10}));

            settings = new RawSettings("2001:0db8:11a3:09d7:1f34:8a2e:07a0:765d");
            binder.Bind<IPAddress>(settings).Should().Be(
                new IPAddress(new byte[] {32,1,  13,184,  17,163,  9,215,  31,52,  138,46,  7,160,  118,93}));
        }

        [Test]
        public void Should_bind_to_IPEndPoint()
        {
            var settings = new RawSettings("192.168.1.10:80");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(new byte[] {192, 168, 1, 10}), 80));
        }

        [Test]
        public void Should_bind_to_DataRate()
        {
            var settings = new RawSettings("10/sec");
            binder.Bind<DataRate>(settings).Should().Be(
                DataRate.FromBytesPerSecond(10));
        }

        [Test]
        public void Should_bind_to_DataSize()
        {
            var settings = new RawSettings("10 bytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromBytes(10));
        }

        [Test]
        public void Should_bind_to_Guid()
        {
            var guid = "936DA01F-9ABD-4d9d-80C7-02AF85C822A8";
            var settings = new RawSettings(guid);
            binder.Bind<Guid>(settings).Should().Be(new Guid(guid));
        }

        [Test]
        public void Should_bind_to_Uri()
        {
            var settings = new RawSettings("http://example.com");
            binder.Bind<Uri>(settings).Should().Be(new Uri("http://example.com", UriKind.Absolute));
        }

        [Test]
        public void Should_bind_to_String()
        {
            var settings = new RawSettings("somestring");
            binder.Bind<string>(settings).Should().Be("somestring");
        }

        [Test]
        public void Should_bind_with_custom_CSTParser()
        {
            var settings = new RawSettings("some,string");
            binder.WithCustomParser<CST>(new CommaSeparatedTextParser())
                .Bind<CST>(settings).Should().BeEquivalentTo(
                    new CST{ Strings = new [] {"some", "string"} });
        }

        [Test]
        public void Should_bind_with_custom_SSTParser()
        {
            var settings = new RawSettings("some;string");
            binder.WithCustomParser<SST>(TryParseSemicolonSeparatedText)
                .Bind<SST>(settings).Should().BeEquivalentTo(
                    new SST{ Strings = new [] {"some", "string"} });
        }

        [Test]
        public void Should_bind_to_NullableInt()
        {
            var settings = new RawSettings("10");
            binder.Bind<int?>(settings).Should().Be(10);

            settings = new RawSettings(null);
            binder.Bind<int?>(settings).Should().Be(null);
        }

        [Test]
        public void Should_bind_to_Enum_ByValueOrName()
        {
            var settings = new RawSettings("10");
            binder.Bind<ConsoleColor>(settings).Should().Be(ConsoleColor.Green);

            settings = new RawSettings("grEEn");
            binder.Bind<ConsoleColor>(settings).Should().Be(ConsoleColor.Green);
        }

        [Test]
        public void Should_bind_to_DateTime()
        {
            var settings = new RawSettings("2018-03-14T15:09:26.535");
            binder.Bind<DateTime>(settings).Should().Be(new DateTime(2018, 3, 14, 15, 9, 26, 535).ToUniversalTime());
        }

        [Test]
        public void Should_bind_to_DateTimeOffset()
        {
            var settings = new RawSettings("2018-03-14T15:09:26.535");
            binder.Bind<DateTimeOffset>(settings).Should().Be(new DateTimeOffset(2018, 3, 14, 15, 9, 26, 535, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)));
        }

        internal struct Struct1
        {
            public int IntValue;
            public string StringValue;
        }
        internal struct Struct2
        {
            public string StringValue;
            public Struct1 Struct1 { get; set; }
            public int IgnoredRoIntProp { get; }
        }

        [Test]
        public void Should_bind_to_Struct()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "IntValue", new RawSettings("10") },
                { "StringValue", new RawSettings("str") },
            });
            binder.Bind<Struct1>(settings)
                .Should().BeEquivalentTo(
                    new Struct1{ IntValue = 10, StringValue = "str" }
                );
        }

        [Test]
        public void Should_bind_to_StructWithStruct()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "StringValue", new RawSettings("strstr") },
                { "IgnoredRoIntProp", new RawSettings("123") },
                { "Struct1", new RawSettings(new Dictionary<string, RawSettings>
                    {
                        { "IntValue", new RawSettings("10") },
                        { "StringValue", new RawSettings("str") },
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

        internal class MyClass
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
            public List<string> PublicStringListProp { get; set; }
            public Dictionary<int, string> PublicDictionaryProp { get; set; }
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
        internal class MyClass2
        {
            public int PublicIntProp { get; set; }
        }
        internal class MyClass3
        {
            public int PublicIntField;
        }

        [Test]
        public void Should_bind_to_Class()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "privateIntField", new RawSettings("10") },
                { "PrivateStrGetProp", new RawSettings("str") },
                { "PublicDoubleSetProp", new RawSettings("1.23") },
                { "PublicIntReadonlyField", new RawSettings("20") },
                { "PrivateConstStringField", new RawSettings("ewq") },
                { "PublicStringStaticField", new RawSettings("statStr") },
                { "PublicIntStaticProp", new RawSettings("1234") },
                { "PublicNullableDoubleSetProp", new RawSettings(null) },
                { "PublicIntArrayProp", new RawSettings(new List<RawSettings>
                {
                    new RawSettings("1"),
                    new RawSettings("2"),
                }) },
                { "PublicStringListProp", new RawSettings(new List<RawSettings>
                {
                    new RawSettings("str1"),
                    new RawSettings("str2"),
                }) },
                { "PublicDictionaryProp", new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "1", new RawSettings("str1") },
                    { "2", new RawSettings("str2") },
                }) },
                { "Struct1", new RawSettings(new Dictionary<string, RawSettings>
                    {
                        { "IntValue", new RawSettings("10") },
                        { "StringValue", new RawSettings("structString") },
                    })
                },
                { "Class2", new RawSettings(new Dictionary<string, RawSettings>
                    {
                        { "PublicIntProp", new RawSettings("111") },
                    })
                },

                { "GetPublicIntReadonlyProp", new RawSettings("321") },
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
                        PublicStringListProp = new List<string> { "str1", "str2" },
                        PublicDictionaryProp = new Dictionary<int, string> { {1, "str1"}, {2, "str2"} },
                    }
                );
        }

        [Test]
        public void Should_bind_to_IntArray()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            binder.Bind<int[]>(settings).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ArrayOfIntArrays()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings(new List<RawSettings>
                {
                    new RawSettings("1"),
                    new RawSettings("2"),
                }),
                new RawSettings(new List<RawSettings>
                {
                    new RawSettings("3"),
                    new RawSettings("4"),
                }),
            });
            binder.Bind<int[][]>(settings).Should().BeEquivalentTo(new [] {1,2}, new[] {3,4});
        }

        [Test]
        public void Should_bind_to_ObjectArray()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "PublicIntProp", new RawSettings("1") },
                }),
                new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "PublicIntProp", new RawSettings("2") },
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
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings("1"),
                new RawSettings("2"),
            });
            binder.Bind<List<int>>(settings).Should().BeEquivalentTo(new List<int> { 1, 2 });
        }

        [Test]
        public void Should_bind_to_ObjectList()
        {
            var settings = new RawSettings(new List<RawSettings>
            {
                new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "PublicIntProp", new RawSettings("1") },
                }),
                new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "PublicIntProp", new RawSettings("2") },
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
        public void Should_bind_to_IntIntDictionary()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "1", new RawSettings("10") },
                { "2", new RawSettings("20") },
            });
            binder.Bind<Dictionary<int, int>>(settings)
                .Should().BeEquivalentTo(
                    new Dictionary<int, int> { {1, 10}, {2, 20} });
        }

        [Test]
        public void Should_bind_to_StringObjectDictionary()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "key1", new RawSettings(new Dictionary<string, RawSettings>
                    {
                        { "PublicIntProp", new RawSettings("1") },
                    })
                },
                { "key2", new RawSettings(new Dictionary<string, RawSettings>
                    {
                        { "PublicIntProp", new RawSettings("2") },
                    })
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

        internal class GenericClass<T1, T2>
        {
            public int PublicIntProp { get; set; }
            public T1 PublicT1Prop { get; set; }
            public T2 PublicT2Prop { get; set; }
            public Dictionary<T1, T2> PublicDictProp { get; set; }
        }

        [Test]
        public void Should_bind_to_GenericClass()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "PublicIntProp", new RawSettings("10") },
                { "PublicT1Prop", new RawSettings("str") },
                { "PublicT2Prop", new RawSettings("1.23") },
                { "PublicDictProp", new RawSettings(new Dictionary<string, RawSettings>
                {
                    { "key1", new RawSettings("1,2") },
                    { "key2", new RawSettings("2.3") },
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

        // === ArgumentNullException

        [Test]
        public void Should_throw_ArgumentNullException_Main()
        {
            new Action(() => binder.Bind<int>(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Dictionary()
        {
            new Action(() => binder.Bind<Struct1>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Array()
        {
            new Action(() => binder.Bind<int[]>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_List()
        {
            new Action(() => binder.Bind<List<int>>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Class()
        {
            new Action(() => binder.Bind<MyClass2>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_Struct()
        {
            new Action(() => binder.Bind<Struct1>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_ArgumentNullException_on_unknown_data_type()
        {
            new Action(() => binder.Bind<CST>(new RawSettings("a,b,c"))).Should().Throw<ArgumentNullException>();
        }

        // === InvalidCastException

        [Test]
        public void Should_throw_InvalidCastException_Primitive_if_wrong_type()
        {
            var settings = new RawSettings("str");
            new Action(() => binder.Bind<int>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_Enum_if_wrong_value()
        {
            var settings = new RawSettings("SeroBuroMalinovy");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();

            settings = new RawSettings("1_000_000");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_struct_field_or_prop_is_absent()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "IntValue", new RawSettings("10") },
                { "WrongName_StringValue", new RawSettings("10") }
            });
            new Action(() => binder.Bind<Struct2>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Should_throw_InvalidCastException_class_field_or_prop_is_absent()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "WrongName_PublicIntProp", new RawSettings("10") }
            });
            new Action(() => binder.Bind<MyClass3>(settings)).Should().Throw<InvalidCastException>();

            settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "WrongName_PublicIntField", new RawSettings("10") }
            });
            new Action(() => binder.Bind<MyClass3>(settings)).Should().Throw<InvalidCastException>();
        }
    }
}