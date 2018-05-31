using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests
{
    // CR(krait): Cache seems to be caching items permanently.. Cache update mechanics must be covered with tests.
    [TestFixture]
    public class ConfigurationProvider_Tests
    {
        private const string TestFile1Name = "test1_ConfigurationProvider.json";
        private const string TestFile2Name = "test2_ConfigurationProvider.json";

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFile1Name);
            File.Delete(TestFile2Name);
        }

        private static void CreateTextFile(int n, string text)
        {
            var fileName = string.Empty;
            switch (n)
            {
                case 1: fileName = TestFile1Name;   break;
                case 2: fileName = TestFile2Name;   break;
            }
            using (var file = new StreamWriter(fileName, false))
                file.WriteLine(text);
        }

        [Test]
        public void Get_WithSourceFor_should_work_correctly()
        {
            CreateTextFile(2, "{ \"Value\": 123 }");
            var provider = new ConfigurationProvider()
                .SetupSourceFor<MyClass>(new JsonFileSource(TestFile2Name));

            provider.IsInCache(typeof(MyClass), null).Should().BeFalse();
            var result = provider.Get<MyClass>();

            provider.IsInCache(typeof(MyClass), result).Should().BeTrue();
            result.Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }

        [Test]
        public void Get_from_source_should_work_correctly()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");
            var provider = new ConfigurationProvider();
            var source = new JsonFileSource(TestFile1Name);

            provider.IsInCache(source, null).Should().BeFalse();
            var result = provider.Get<MyClass>(source);

            provider.IsInCache(source, result).Should().BeTrue();
            result.Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }

        [Test]
        public void Should_throw_on_no_needed_sources()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");

            new Action(() => new ConfigurationProvider()
                .Get<MyClass>())
                .Should().Throw<ArgumentException>();

            new Action(() => new ConfigurationProvider()
                .SetupSourceFor<int>(new JsonFileSource(TestFile1Name))
                .Get<MyClass>())
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_throw_on_having_observer()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");

            new Action(() =>
                {
                    var x = new ConfigurationProvider().SetupSourceFor<int>(new JsonFileSource(TestFile1Name));
                    x.Observe<int>();
                    x.SetupSourceFor<string>(new JsonFileSource(TestFile1Name));
                })
                .Should().Throw<InvalidOperationException>();
        }

        /*[Test]
        public void Get_should_call_back_on_error()
        {
            CreateTextFile(1, "{ \"Value\": 123.45 }");

            var res = false;
            Action<Exception> Cb() => exception => res = true;

            new ConfigurationProvider(null, false, Cb()).Get<MyClass>();
            res.Should().BeTrue();

            res = false;
            new ConfigurationProvider(null, false, Cb())
                .SetupSourceFor<int>(new JsonFileSource(TestFile1Name))
                .Get<MyClass>();
            res.Should().BeTrue();

            res = false;
            new ConfigurationProvider(null, false, Cb())
                .Get<int>(new JsonFileSource(TestFile1Name));
            res.Should().BeTrue();
        }*/

        [Test]
        public void Should_only_call_OnNext_on_observers_of_the_type_whose_underlying_source_was_updated()
        {
            new Action(() =>
                    CountOnNextCallsForTwoSources().Should().Be((2, 1)))
                .ShouldPassIn(1.Seconds());
        }

        private (int vClass1, int vClass2) CountOnNextCallsForTwoSources()
        {
            CreateTextFile(1, "{ \"Value\": 1 }");
            CreateTextFile(2, "{ \"Value\": 123 }");
            var vClass1 = 0;
            var vClass2 = 0;
            using (var jcs1 = new JsonFileSource(TestFile1Name))
            using (var jcs2 = new JsonFileSource(TestFile2Name))
            {
                var cp = new ConfigurationProvider()
                    .SetupSourceFor<MyClass>(jcs1)
                    .SetupSourceFor<MyClass2>(jcs2);
                
                var sub1 = cp.Observe<MyClass>().Subscribe(val =>
                {
                    vClass1++;
                    val.Value.Should().Be(vClass1);
                });
                var sub2 = cp.Observe<MyClass2>().Subscribe(val => vClass2++);

                Thread.Sleep(200.Milliseconds());
                CreateTextFile(1, "{ \"Value\": 2 }");
                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
            }
            return (vClass1, vClass2);
        }

        [Test]
        public void Should_Observe_file_by_source()
        {
            new Action(() => 
                    ObserveFileBySource().Should().Be(1))
                .ShouldPassIn(1.Seconds());
        }

        private static int ObserveFileBySource()
        {
            CreateTextFile(1, "{ \"Value\": 0 }");
            var val = 0;
            using (var jcs = new JsonFileSource(TestFile1Name))
            {
                var cp = new ConfigurationProvider();
                var sub = cp.Observe<MyClass>(jcs).Subscribe(cl =>
                {
                    val++;
                    cl.Value.Should().Be(val);
                });

                Thread.Sleep(200.Milliseconds());
                CreateTextFile(1, "{ \"Value\": 1 }");
                Thread.Sleep(200.Milliseconds());

                sub.Dispose();

                CreateTextFile(1, "{ \"Value\": 2 }");
                Thread.Sleep(200.Milliseconds());
            }
            return val;
        }

        private class MyClass
        {
            public int Value { get; set; }

            public override string ToString() => Value.ToString();
        }

        private class MyClass2
        {
            public int Value { get; set; }

            public override string ToString() => Value.ToString();
        }

        [Test]
        public void Should_read_from_cache_for_one_source()
        {
            var res = new MyClass { Value = 0 };
            var cs = Substitute.For<IConfigurationSource>();
            cs.Get().Returns(
                x => new RawSettings(new OrderedDictionary
                {
                    {"Value", new RawSettings("0", "Value")},
                }, "root"),
                x => throw new Exception("Only one execution is allowed. Second one must be from cache."));

            var cp = new ConfigurationProvider()
                .SetupSourceFor<MyClass>(cs);

            cp.IsInCache(typeof(MyClass), null).Should().BeFalse();
            var result = cp.Get<MyClass>();
            result.Should().BeEquivalentTo(res);

            cp.IsInCache(typeof(MyClass), result).Should().BeTrue();
            cp.Get<MyClass>().Should().BeEquivalentTo(res);   //from cache

            cs.Dispose();
        }

        [Test]
        public void Should_read_from_cache_for_multiple_sources()
        {
            var res = new MyClass { Value = 0 };
            var ret = new RawSettings(
                new OrderedDictionary
                {
                    {"Value", new RawSettings("0", "Value")},
                }, "root");

            var cs1 = Substitute.For<IConfigurationSource>();
            cs1.Get().Returns(
                x => ret,   //on the second SetupSourceFor
                x => throw new Exception("Only one execution is allowed (source 1). The second one must be from cache."));

            var cs2 = Substitute.For<IConfigurationSource>();
            cs2.Get().Returns(
                x => ret,   //on the second SetupSourceFor
                x => throw new Exception("Only one execution is allowed (source 2). The second one must be from cache."));

            var cp = new ConfigurationProvider()
                .SetupSourceFor<MyClass>(cs1)
                .SetupSourceFor<MyClass>(cs2);

            cp.IsInCache(typeof(MyClass), null).Should().BeFalse();
            var result = cp.Get<MyClass>();
            result.Should().BeEquivalentTo(res);

            cp.IsInCache(typeof(MyClass), result).Should().BeTrue();
            cp.Get<MyClass>().Should().BeEquivalentTo(res); //from cache

            cs1.Dispose();
            cs2.Dispose();
        }
    }
}