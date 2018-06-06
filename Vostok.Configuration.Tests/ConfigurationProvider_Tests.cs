using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Conversions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigurationProvider_Tests
    {
        private const string TestName = nameof(ConfigurationProvider);
        
        public class ByType
        {
            [TearDown]
            public void Cleanup() => TestHelper.DeleteAllFiles(TestName);

            [Test]
            public void Get_WithSourceFor_should_work_correctly()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider()
                    .SetupSourceFor<MyClass>(new JsonFileSource(fileName));

                var result = provider.Get<MyClass>();
                result.Should().BeEquivalentTo(new MyClass {Value = 123});
            }

            [Test]
            public void SetupSourceFor_should_throw_if_add_source_for_type_which_Get_was_invoked()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider();
                var source = new JsonFileSource(fileName);

                provider.SetupSourceFor<MyClass>(source); //new type
                provider.Get<MyClass>();
                provider.SetupSourceFor<int>(source); //new type

                var source2 = new JsonFileSource(fileName);
                provider.SetupSourceFor<int>(source2); //no Get() or Observe()
                new Action(() => provider.SetupSourceFor<MyClass>(source2)).Should().Throw<InvalidOperationException>(); //was Get()
            }

            [Test]
            public void SetupSourceFor_should_throw_if_add_source_for_type_which_Observe_was_invoked()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider();
                var source = new JsonFileSource(fileName);

                provider.SetupSourceFor<MyClass>(source); //new type
                provider.Observe<MyClass>().Subscribe();
                provider.SetupSourceFor<int>(source); //new type

                var source2 = new JsonFileSource(fileName);
                provider.SetupSourceFor<int>(source2); //no Get() or Observe()
                new Action(() => provider.SetupSourceFor<MyClass>(source2)).Should().Throw<InvalidOperationException>(); //was Observe()
            }

            [Test]
            public void Should_update_value_in_cache_on_file_change()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider();
                var source = new JsonFileSource(fileName);

                provider.SetupSourceFor<MyClass>(source);
                var result1 = provider.Get<MyClass>();
                result1.Should().BeEquivalentTo(new MyClass {Value = 123});

                TestHelper.CreateFile(TestName, "{ 'Value': 321 }", fileName);
                var result2 = provider.Get<MyClass>();
                result2.Should().BeEquivalentTo(new MyClass {Value = 123}, "cache is not updated yet");
                result1.Should().Be(result2, "read from cache");

                Thread.Sleep(200.Milliseconds());

                result2 = provider.Get<MyClass>();
                result2.Should().BeEquivalentTo(new MyClass {Value = 321}, "cache was updated");
            }

            [Test]
            public void Get_should_throw_on_no_needed_sources()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");

                new Action(() => new ConfigurationProvider()
                    .Get<MyClass>())
                .Should()
                .Throw<ArgumentException>();

                new Action(() => new ConfigurationProvider()
                    .SetupSourceFor<int>(new JsonFileSource(fileName))
                    .Get<MyClass>())
                .Should()
                .Throw<ArgumentException>();
            }

            [Test]
            public void Should_only_call_OnNext_for_observers_of_the_type_whose_underlying_source_was_updated()
            {
                new Action(() =>
                    CountOnNextCallsForTwoSources().Should().Be((2, 1)))
                .ShouldPassIn(1.Seconds());
            }

            private (int vClass1, int vClass2) CountOnNextCallsForTwoSources()
            {
                var fileName1 = TestHelper.CreateFile(TestName, "{ 'Value': 1 }");
                var fileName2 = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var vClass1 = 0;
                var vClass2 = 0;
                using (var jcs1 = new JsonFileSource(fileName1))
                using (var jcs2 = new JsonFileSource(fileName2))
                {
                    var cp = new ConfigurationProvider()
                        .SetupSourceFor<MyClass>(jcs1)
                        .SetupSourceFor<MyClass2>(jcs2);

                    var sub1 = cp.Observe<MyClass>()
                        .Subscribe(
                            val =>
                            {
                                vClass1++;
                                val.Value.Should().Be(vClass1);
                            });
                    var sub2 = cp.Observe<MyClass2>().Subscribe(val => vClass2++);

                    Thread.Sleep(200.Milliseconds());
                    TestHelper.CreateFile(TestName, "{ 'Value': 2 }", fileName1);
                    Thread.Sleep(200.Milliseconds());

                    sub1.Dispose();
                    sub2.Dispose();
                }

                return (vClass1, vClass2);
            }

            [Test]
            public void Should_read_from_cache_for_multiple_sources()
            {
                var fileName1 = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var fileName2 = TestHelper.CreateFile(TestName, "{ 'Value': 321 }");
                var source1 = new JsonFileSource(fileName1);
                var source2 = new JsonFileSource(fileName2);

                var cp = new ConfigurationProvider()
                    .SetupSourceFor<MyClass>(source1)
                    .SetupSourceFor<MyClass>(source2);

                var result1 = cp.Get<MyClass>();
                result1.Should().BeEquivalentTo(new MyClass {Value = 321}, "first read");

                var result2 = cp.Get<MyClass>();
                result2.Should().Be(result1, "from cache");

                source1.Dispose();
                source2.Dispose();
            }

            [Test]
            public void Should_throw_exception_on_Get_with_default_settings()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var cp = new ConfigurationProvider()
                    .SetupSourceFor<int>(source);
                new Action(() => cp.Get<int>()).Should().Throw<Exception>();
            }

            [Test]
            public void Should_return_default_value_if_disabled_throwing_exceptions()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var cp = new ConfigurationProvider(new ConfigurationProviderSettings{ ThrowExceptions = false })
                    .SetupSourceFor<int>(source);
                cp.Get<int>().Should().Be(default);
            }

            [Test]
            public void Should_return_default_value_and_invoke_OnError_by_settings()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var msg = string.Empty;
                var cp = new ConfigurationProvider(new ConfigurationProviderSettings
                    {
                        ThrowExceptions = false,
                        OnError = e => msg = e.Message,
                    })
                    .SetupSourceFor<int>(source);
                cp.Get<int>().Should().Be(default);
                msg.Should().NotBeNullOrWhiteSpace();
            }

            [Test]
            public void Should_read_from_cache_in_case_of_exception_if_disabled_throwing_exceptions()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var source = new JsonFileSource(fileName);
                var msg = string.Empty;
                var cp = new ConfigurationProvider(new ConfigurationProviderSettings
                    {
                        ThrowExceptions = false,
                        OnError = e => msg = e.Message,
                    })
                    .SetupSourceFor<int>(source);
                cp.Get<int>().Should().Be(123);

                TestHelper.CreateFile(TestName, "{ 'Value': 'str' }", fileName);
                Thread.Sleep(200.Milliseconds());

                cp.Get<int>().Should().Be(123);
                msg.Should().NotBeNullOrWhiteSpace();
            }
        }

        public class BySource
        {
            [TearDown]
            public void Cleanup() => TestHelper.DeleteAllFiles(TestName);

            [Test]
            public void Get_from_source_should_work_correctly()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider();
                var source = new JsonFileSource(fileName);

                var result = provider.Get<MyClass>(source);
                result.Should().BeEquivalentTo(new MyClass {Value = 123});
            }

            [Test]
            public void Should_update_value_in_cache_on_file_change()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var provider = new ConfigurationProvider();
                var source = new JsonFileSource(fileName);

                var result1 = provider.Get<MyClass>(source);
                result1.Should().BeEquivalentTo(new MyClass {Value = 123});

                TestHelper.CreateFile(TestName, "{ 'Value': 321 }", fileName);
                var result2 = provider.Get<MyClass>(source);
                result2.Should().BeEquivalentTo(new MyClass {Value = 123}, "cache is not updated yet");
                result1.Should().Be(result2, "read from cache");

                Thread.Sleep(200.Milliseconds());

                result2 = provider.Get<MyClass>(source);
                result2.Should().BeEquivalentTo(new MyClass {Value = 321}, "cache was updated");
            }

            [Test]
            public void Should_only_call_OnNext_for_observers_of_the_type_whose_underlying_source_was_updated()
            {
                new Action(() =>
                    CountOnNextCallsForTwoSources().Should().Be((2, 1)))
                .ShouldPassIn(1.Seconds());
            }

            private (int vClass1, int vClass2) CountOnNextCallsForTwoSources()
            {
                var fileName1 = TestHelper.CreateFile(TestName, "{ 'Value': 1 }");
                var fileName2 = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var vClass1 = 0;
                var vClass2 = 0;
                using (var jcs1 = new JsonFileSource(fileName1))
                using (var jcs2 = new JsonFileSource(fileName2))
                {
                    var cp = new ConfigurationProvider();

                    var sub1 = cp.Observe<MyClass>(jcs1)
                        .Subscribe(
                            val =>
                            {
                                vClass1++;
                                val.Value.Should().Be(vClass1);
                            });
                    var sub2 = cp.Observe<MyClass2>(jcs2).Subscribe(val => vClass2++);

                    Thread.Sleep(200.Milliseconds());
                    TestHelper.CreateFile(TestName, "{ 'Value': 2 }", fileName1);
                    Thread.Sleep(200.Milliseconds());

                    sub1.Dispose();
                    sub2.Dispose();
                }

                return (vClass1, vClass2);
            }

            [Test]
            public void Should_Observe_file()
            {
                new Action(() =>
                    ObserveFile().Should().Be(1))
                .ShouldPassIn(1.Seconds());
            }

            private static int ObserveFile()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 0 }");
                var val = 0;
                using (var jcs = new JsonFileSource(fileName))
                {
                    var cp = new ConfigurationProvider();
                    var sub = cp.Observe<MyClass>(jcs)
                        .Subscribe(
                            cl =>
                            {
                                val++;
                                cl.Value.Should().Be(val);
                            });

                    Thread.Sleep(200.Milliseconds());
                    TestHelper.CreateFile(TestName, "{ 'Value': 1 }", fileName);
                    Thread.Sleep(200.Milliseconds());

                    sub.Dispose();

                    TestHelper.CreateFile(TestName, "{ 'Value': 2 }", fileName);
                    Thread.Sleep(200.Milliseconds());
                }

                return val;
            }

            [Test]
            public void Should_throw_exception_on_Get_with_default_settings()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var cp = new ConfigurationProvider();
                new Action(() => cp.Get<int>(source)).Should().Throw<Exception>();
            }

            [Test]
            public void Should_return_default_value_if_disabled_throwing_exceptions()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var cp = new ConfigurationProvider(new ConfigurationProviderSettings { ThrowExceptions = false });
                cp.Get<int>(source).Should().Be(default);
            }

            [Test]
            public void Should_return_default_value_and_invoke_OnError_by_settings()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 'str' }");
                var source = new JsonFileSource(fileName);
                var msg = string.Empty;
                var cp = new ConfigurationProvider(
                    new ConfigurationProviderSettings
                    {
                        ThrowExceptions = false,
                        OnError = e => msg = e.Message,
                    });
                cp.Get<int>(source).Should().Be(default);
                msg.Should().NotBeNullOrWhiteSpace();
            }

            [Test]
            public void Should_read_from_cache_in_case_of_exception_if_disabled_throwing_exceptions()
            {
                var fileName = TestHelper.CreateFile(TestName, "{ 'Value': 123 }");
                var source = new JsonFileSource(fileName);
                var msg = string.Empty;
                var cp = new ConfigurationProvider(
                    new ConfigurationProviderSettings
                    {
                        ThrowExceptions = false,
                        OnError = e => msg = e.Message,
                    });
                cp.Get<int>(source).Should().Be(123);

                TestHelper.CreateFile(TestName, "{ 'Value': 'str' }", fileName);
                Thread.Sleep(200.Milliseconds());

                cp.Get<int>(source).Should().Be(123);
                msg.Should().NotBeNullOrWhiteSpace();
            }
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
    }
}