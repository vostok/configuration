using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
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
        public void Get_should_throw_on_no_sources()
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
        public void Get_should_call_back_on_error()
        {
            CreateTextFile(1, "{ \"Value\": 123 }");

            var res = false;
            Action<Exception> Cb() => exception => res = true;

            new ConfigurationProvider(null, false, Cb()).Get<MyClass>();
            res.Should().BeTrue();

            res = false;
            new ConfigurationProvider(null, false, Cb())
                .WithSourceFor<int>(new JsonFileSource(TestFile1Name))
                .Get<MyClass>();
            res.Should().BeTrue();

            res = false;
            new ConfigurationProvider(null, false, Cb())
                .Get<int>(new JsonFileSource(TestFile1Name));
            res.Should().BeTrue();
        }

        [Test, Explicit("Not stable on mass tests")]
        public void Should_only_call_OnNext_on_observers_of_the_type_whose_underlying_source_was_updated()
        {
            new Action(() =>
                    CountOnNextCallsForTwoSources().Should().Be((2, 0)))
                .ShouldPassIn(1.Seconds());
        }

        private (int vClass, int vInt) CountOnNextCallsForTwoSources()
        {
            CreateTextFile(1, "{ \"Value\": 1 }");
            CreateTextFile(2, "{ \"Value\": 123 }");
            var vClass = 0;
            var vInt = 0;
            using (var jcs1 = new JsonFileSource(TestFile1Name, 100.Milliseconds()))
            using (var jcs2 = new JsonFileSource(TestFile2Name, 100.Milliseconds()))
            {
                var cp = new ConfigurationProvider()
                    .WithSourceFor<MyClass>(jcs1);

                var sub1 = cp.Observe<MyClass>().Subscribe(val =>
                {
                    vClass++;
                    val.Value.Should().Be(vClass);
                });
                var sub2 = cp.Observe<int>().Subscribe(val => vInt++);

                cp.WithSourceFor<int>(jcs2);

                Thread.Sleep(200.Milliseconds());
                CreateTextFile(1, "{ \"Value\": 2 }");
                Thread.Sleep(200.Milliseconds());

                sub1.Dispose();
                sub2.Dispose();
                Thread.Sleep(200.Milliseconds());
            }
            SettingsFileWatcher.StopAndClear();
            return (vClass, vInt);
        }

        [Test, Explicit("Not stable on mass tests")]
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
            using (var jcs = new JsonFileSource(TestFile1Name, 100.Milliseconds()))
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
            SettingsFileWatcher.StopAndClear();
            return val;
        }

        private class MyClass
        {
            public int Value { get; set; }
        }
    }
}