using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Primitives;
using Vostok.Configuration.Printing;
using Vostok.Configuration.Sources.Json;

namespace Vostok.Configuration.Tests
{
    internal class ConfigurationPrinter_Tests
    {
        [Test]
        public void Should_print_complex_objects()
        {
            var settings = new MyClass1
            {
                Item = new MyClass2
                {
                    Str = "asdf",
                    Dict = new Dictionary<string, MyClass3>
                    {
                        ["k1"] = new MyClass3 {Num = 1},
                        ["K2"] = new MyClass3 {Num = 2}
                    }
                },
                Array = new[] {new MyClass3 {Num = 3}, new MyClass3 {Num = 4}}
            };

            var result = PrintAndParse(settings);

            result.Should()
                .Be(
                    @"{
   ""Item"": 
   {
      ""Str"": ""asdf"",
      ""Dict"": 
      {
         ""k1"": 
         {
            ""Num"": ""1""
         },
         ""K2"": 
         {
            ""Num"": ""2""
         }
      }
   },
   ""Array"": 
   [
      {
         ""Num"": ""3""
      },
      {
         ""Num"": ""4""
      }
   ]
}");
        }

        [Test]
        public void Should_print_empty_objects()
        {
            var settings = new MyClass1();

            var result = PrintAndParse(settings);

            result.Should()
                .Be(
                    @"{
   ""Item"": null,
   ""Array"": null
}");
        }
        
        [Test]
        public void Should_not_print_secrets()
        {
            var settings = new MyClass4
            {
                Class5 = new MyClass5
                {
                    Secret5 = "555"
                },
                Public4 = "p444",
                Secret4 = "s444"
            };

            var result = PrintAndParse(settings, false);

            result.Should()
                .Be(
                    @"{
   ""Class5"": <secret>,
   ""Public4"": ""p444"",
   ""Secret4"": <secret>
}");
        }

        [Test]
        public void Should_print_all_types()
        {
            var settings = new MyClass8();

            var result = PrintAndParse(settings);

            result.Should().Be(@"{
   ""S0"": null,
   ""S1"": """",
   ""S2"": ""asdf"",
   ""C"": ""z"",
   ""B"": ""True"",
   ""B1"": ""33"",
   ""B2"": ""34"",
   ""S3"": ""35"",
   ""S4"": ""36"",
   ""I1"": ""37"",
   ""I2"": ""38"",
   ""L1"": ""39"",
   ""L2"": ""40"",
   ""F"": ""41,5"",
   ""D"": ""42,5"",
   ""D2"": ""43,5"",
   ""G"": ""bd9cbd49-4c5c-4cbb-9b2b-1062c07b29c2"",
   ""U"": ""https://github.com/vostok?q=asdf&type=all&language=&sort="",
   ""T"": ""00:00:44.5000000"",
   ""O"": ""2018-03-14T15:09:26.5350000+05:00"",
   ""O2"": ""2018-03-14T15:09:26.0000000+03:30"",
   ""I"": ""2001:db8:11a3:9d7:1f34:8a2e:7a0:765d"",
   ""I3"": ""148.136.1.0:80"",
   ""D3"": ""45 B"",
   ""D4"": ""46 B/sec"",
   ""E"": ""utf-32"",
   ""E2"": ""B"",
   ""I4"": ""47"",
   ""I5"": null
}");
        }
        
        [Test]
        public void Should_use_ToString()
        {
            var settings = new MyClass6
            {
                Str = "asdf"
            };

            var result = PrintAndParse(settings);

            result.Should().Be(@"""!!! asdf""");
        }
        
        [Test]
        public void Should_not_use_ToString_without_Parse()
        {
            var settings = new MyClass6
            {
                Str = "asdf"
            };

            var result = PrintAndParse(settings);

            result.Should().Be(@"""!!! asdf""");
        }
        
        private static string PrintAndParse<T>(T settings, bool parse = true)
        {
            var result = ConfigurationPrinter.Print(settings, new PrintSettings {Format = PrintFormat.JSON});

            if (!parse)
                return result;
            
            using (var p = new ConfigurationProvider())
            {
                var parsed = p.Get<T>(new JsonStringSource(result));
                parsed.Should().BeEquivalentTo(settings);
            }

            return result;
        }
        
        internal class MyClass1
        {
            public MyClass2 Item { get; set; }
            public MyClass3[] Array { get; set; }
        }

        internal class MyClass2
        {
            public string Str { get; set; }
            public Dictionary<string, MyClass3> Dict { get; set; }
        }

        internal class MyClass3
        {
            public int Num { get; set; }
        }

        internal class MyClass4
        {
            public MyClass5 Class5 { get; set; }
            
            public string Public4 { get; set; }
            
            [Secret]
            public string Secret4 { get; set; }
        }

        [Secret]
        internal class MyClass5
        {
            public string Secret5 { get; set; }
        }

        internal class MyClass6
        {
            public string Str { get; set; }

            public static MyClass6 Parse(string input) => new MyClass6
            {
                Str = input.Substring(4)
            };
            
            public override string ToString() =>
                $"!!! {Str}";
        }
        
        internal class MyClass7
        {
            public string Str { get; set; }

            public override string ToString() =>
                $"!!! {Str}";
        }

        class MyClass8
        {
            public string S0 { get; set; } = null;
            public string S1 { get; set; } = "";
            public string S2 { get; set; } = "asdf";
            public char C { get; set; } = 'z';
            public bool B { get; set; } = true;
            public byte B1 { get; set; } = 33;
            public sbyte B2 { get; set; } = 34;
            public short S3 { get; set; } = 35;
            public ushort S4 { get; set; } = 36;
            public int I1 { get; set; } = 37;
            public uint I2 { get; set; } = 38;
            public long L1 { get; set; } = 39;
            public ulong L2 { get; set; } = 40;
            public float F { get; set; } = 41.5f;
            public double D { get; set; } = 42.5;
            public decimal D2 { get; set; } = 43.5m;
            public Guid G { get; set; } = new Guid("bd9cbd49-4c5c-4cbb-9b2b-1062c07b29c2");
            public Uri U { get; set; } = new Uri("https://github.com/vostok?q=asdf&type=all&language=&sort=");
            public TimeSpan T { get; set; } = 44.5.Seconds();
            public DateTimeOffset O { get; set; } = DateTimeOffset.Parse("2018-03-14 15:09:26.535");
            public DateTimeOffset O2 { get; set; } = DateTimeOffset.Parse("2018-03-14 15:09:26 +03:30");
            public IPAddress I { get; set; } = IPAddress.Parse("2001:0db8:11a3:09d7:1f34:8a2e:07a0:765d");
            public IPEndPoint I3 { get; set; } = new IPEndPoint(IPAddress.Parse("192.168.1.10"), 80);
            public DataSize D3 { get; set; } = DataSize.FromBytes(45);
            public DataRate D4 { get; set; } = DataRate.FromBytesPerSecond(46);
            public Encoding E { get; set; } = Encoding.UTF32;
            public MyEnum E2 { get; set; } = MyEnum.B;
            public int? I4 { get; set; } = 47;
            public int? I5 { get; set; } = null;
        }

        enum MyEnum
        {
            A,
            B
        }
    }
}