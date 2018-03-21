using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Convertions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests
{
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
                case 1:
                    fileName = TestFile1Name;
                    break;
                case 2:
                    fileName = TestFile2Name;
                    break;
            }
            using (var file = new StreamWriter(fileName, false))
                file.WriteLine(text);
        }

        class MyClass
        {
            public int Value { get; set; }
        }
        [Test]
        public void Get_WithSourceFor_should_work_correctly()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");
            new ConfigurationProvider()
                .WithSourceFor<MyClass>(new JsonFileSource(TestFile1Name))
                .Get<MyClass>()
                .Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }

        [Test]
        public void Get_from_source_should_work_correctly()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");
            new ConfigurationProvider()
                .Get<MyClass>(new JsonFileSource(TestFile1Name))
                .Should().BeEquivalentTo(new MyClass{ Value = 123 });
        }

        [Test]
        public void Get_source_throw_on_no_sources()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");

            new Action(() => new ConfigurationProvider()
                .Get<MyClass>())
                .Should().Throw<ArgumentException>();

            new Action(() => new ConfigurationProvider()
                .WithSourceFor<int>(new JsonFileSource(TestFile1Name))
                .Get<MyClass>())
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_Observe_file_1_and_not_Observe_file_2()
        {
            new Action(() => 
                    Should_Observe_file_1_and_not_Observe_file_2_test().Should().Be((2, 0)))
                .ShouldPassIn(3.Seconds());
        }

        private (int vClass, int vInt) Should_Observe_file_1_and_not_Observe_file_2_test()
        {
            CreateTextFile(1, "{ \"Value\": 1 }");
            CreateTextFile(2, "{ \"Value\": 123 }");
            var vClass = 0;
            var vInt = 0;
            var jcs1 = new JsonFileSource(TestFile1Name, 300.Milliseconds());
            var jcs2 = new JsonFileSource(TestFile2Name, 300.Milliseconds());
            var cp = new ConfigurationProvider()
                .WithSourceFor<MyClass>(jcs1);

            var sub1 = cp.Observe<MyClass>().Subscribe(val =>
            {
                vClass++;
                val.Value.Should().Be(vClass);
            });
            var sub2 = cp.Observe<int>().Subscribe(val => vInt++);

            cp.WithSourceFor<int>(jcs2);

            Thread.Sleep(1.Seconds());
            CreateTextFile(1, "{ \"Value\": 2 }");
            Thread.Sleep(1.Seconds());

            sub1.Dispose();
            sub2.Dispose();
            return (vClass, vInt);
        }

        [Test]
        public void Should_Observe_file_by_source()
        {
            new Action(() => 
                    Should_Observe_file_by_source_test().Should().Be(1))
                .ShouldPassIn(4.Seconds());
        }

        private int Should_Observe_file_by_source_test()
        {
            CreateTextFile(1, "{ \"Value\": 0 }");
            var val = 0;
            var jcs = new JsonFileSource(TestFile1Name, 300.Milliseconds());
            var cp = new ConfigurationProvider();

            var sub = cp.Observe<MyClass>(jcs).Subscribe(cl =>
            {
                val++;
                cl.Value.Should().Be(val);
            });

            Thread.Sleep(1.Seconds());
            CreateTextFile(1, "{ \"Value\": 1 }");
            Thread.Sleep(1.Seconds());

            sub.Dispose();

            CreateTextFile(1, "{ \"Value\": 2 }");
            Thread.Sleep(1.Seconds());

            return val;
        }
    }
}