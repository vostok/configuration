using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;

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
            var res = new TimeSpan(1, 0, 0, 0);
            var settings = new RawSettings("1d");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 day");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1   days");
            binder.Bind<TimeSpan>(settings).Should().Be(res);

            res = new TimeSpan(0, 1, 0, 0);
            settings = new RawSettings("1h");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 hour");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1   hours");
            binder.Bind<TimeSpan>(settings).Should().Be(res);

            res = new TimeSpan(0, 0, 1, 0);
            settings = new RawSettings("1m");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 min");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 minute");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1   minutes");
            binder.Bind<TimeSpan>(settings).Should().Be(res);

            res = new TimeSpan(0, 0, 0, 1);
            settings = new RawSettings("1s");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 sec");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 second");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1   seconds");
            binder.Bind<TimeSpan>(settings).Should().Be(res);

            res = new TimeSpan(0, 0, 0, 0, 1);
            settings = new RawSettings("1ms");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 msec");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1 millisecond");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
            settings = new RawSettings("1   milliseconds");
            binder.Bind<TimeSpan>(settings).Should().Be(res);
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
            var settings = new RawSettings("127.0.0.1");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 0));

            settings = new RawSettings("192.168.1.10:80");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(new byte[] {192, 168, 1, 10}), 80));

            var ipV6 = "2001:0db8:11a3:09d7:1f34:8a2e:07a0:765d";
            var ipV6Bytes = new byte[] { 32, 1, 13, 184, 17, 163, 9, 215, 31, 52, 138, 46, 7, 160, 118, 93 };

            settings = new RawSettings(ipV6);
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(ipV6Bytes), 0));

            settings = new RawSettings($"[{ipV6}]");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(ipV6Bytes), 0));

            settings = new RawSettings($"[{ipV6}]:80");
            binder.Bind<IPEndPoint>(settings).Should().Be(
                new IPEndPoint(new IPAddress(ipV6Bytes), 80));
        }

        [TestCase("10 /s", 10)]
        [TestCase("10/sec", 10)]
        [TestCase("10 /second", 10)]
        public void Should_bind_to_DataRate(string input, int seconds)
        {
            var settings = new RawSettings(input);
            binder.Bind<DataRate>(settings).Should().Be(
                DataRate.FromBytesPerSecond(seconds));
        }

        [Test]
        public void Should_bind_to_DataSize()
        {
            var settings = new RawSettings("1b");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromBytes(1));
            settings = new RawSettings("10 bytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromBytes(10));

            settings = new RawSettings("1KB");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromKilobytes(1));
            settings = new RawSettings("10 KiLoByTeS");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromKilobytes(10));

            settings = new RawSettings("1Mb");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromMegabytes(1));
            settings = new RawSettings("1030   megabytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromMegabytes(1030));

            settings = new RawSettings("1gb");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromGigabytes(1));
            settings = new RawSettings("10gigabytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromGigabytes(10));

            settings = new RawSettings("1tb");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromTerabytes(1));
            settings = new RawSettings("10terabytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromTerabytes(10));

            settings = new RawSettings("1pb");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromPetabytes(1));
            settings = new RawSettings("10petabytes");
            binder.Bind<DataSize>(settings).Should().Be(DataSize.FromPetabytes(10));
        }

        [Test]
        public void Should_bind_to_Guid()
        {
            var guid = "936DA01F-9ABD-4d9d-80C7-02AF85C822A8";
            var settings = new RawSettings(guid);
            binder.Bind<Guid>(settings).Should().Be(new Guid(guid));
        }

        [TestCase("http://example.com", UriKind.Absolute)]
        [TestCase("example.com/some", UriKind.RelativeOrAbsolute)]
        [TestCase("/part/of/path", UriKind.Relative)]
        public void Should_bind_to_Uri(string uri, UriKind kind)
        {
            var settings = new RawSettings(uri);
            binder.Bind<Uri>(settings).Should().Be(new Uri(uri, kind));
        }

        [Test]
        public void Should_bind_to_String()
        {
            var settings = new RawSettings("somestring");
            binder.Bind<string>(settings).Should().Be("somestring");
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

        [TestCase("2018-03-14 15:09:26.535", 2018, 3, 14, 15, 9, 26, 535)]   //for Stephen Hawking in a day of Pi
        [TestCase("2018-03-14T15:09:26.535", 2018, 3, 14, 15, 9, 26, 535)]
        [TestCase("20050809T181142+0330", 2005, 8, 9, 18 + 3 - 1/*?*/, 11 + 30, 42, 0)]
        [TestCase("20050809T181142", 2005, 8, 9, 18, 11, 42, 0)]
        [TestCase("20050809", 2005, 8, 9, 0, 0, 0, 0)]
        [TestCase("2005/08/09", 2005, 8, 9, 0, 0, 0, 0)]
        [TestCase("2005.08.09", 2005, 8, 9, 0, 0, 0, 0)]
        [TestCase("11:22:33", 0, 0, 0, 11, 22, 33, 0)]
        [TestCase("11:22:33.044", 0, 0, 0, 11, 22, 33, 44)]
        [TestCase("112233", 0, 0, 0, 11, 22, 33, 0)]
        public void Should_bind_to_DateTime(string value, int y, int m, int d, int h, int min, int s, int ms)
        {
            var settings = new RawSettings(value);
            if (y == 0 && m == 0 && d == 0)
            {
                y = DateTime.Now.Year;
                m = DateTime.Now.Month;
                d = DateTime.Now.Day;
            }
            binder.Bind<DateTime>(settings).Should().Be(new DateTime(y, m, d, h, min, s, ms));
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
        public void Throw_ArgumentNullException_Main()
        {
            new Action(() => binder.Bind<int>(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Throw_ArgumentNullException_Dictionary()
        {
            new Action(() => binder.Bind<Struct1>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Throw_ArgumentNullException_Array()
        {
            new Action(() => binder.Bind<int[]>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Throw_ArgumentNullException_List()
        {
            new Action(() => binder.Bind<List<int>>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Throw_ArgumentNullException_Class()
        {
            new Action(() => binder.Bind<MyClass2>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Throw_ArgumentNullException_Struct()
        {
            new Action(() => binder.Bind<Struct1>(new RawSettings(null))).Should().Throw<ArgumentNullException>();
        }

        // === InvalidCastException

        [Test]
        public void Throw_InvalidCastException_Primitive_if_wrong_type()
        {
            var settings = new RawSettings("str");
            new Action(() => binder.Bind<int>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Throw_InvalidCastException_Enum_if_wrong_value()
        {
            var settings = new RawSettings("SeroBuroMalinovy");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();

            settings = new RawSettings("1_000_000");
            new Action(() => binder.Bind<ConsoleColor>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Throw_InvalidCastException_struct_field_or_prop_is_absent()
        {
            var settings = new RawSettings(new Dictionary<string, RawSettings>
            {
                { "IntValue", new RawSettings("10") },
                { "WrongName_StringValue", new RawSettings("10") }
            });
            new Action(() => binder.Bind<Struct2>(settings)).Should().Throw<InvalidCastException>();
        }
        
        [Test]
        public void Throw_InvalidCastException_class_field_or_prop_is_absent()
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