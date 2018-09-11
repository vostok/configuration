using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.Validation;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class Additional_Tests
    {
        private const string TestName = nameof(Additional_Tests);
        private static IDisposable subscription;
        private bool fileRead;

        [TearDown]
        public void Cleanup() => TestHelper.DeleteAllFiles(TestName);

        [Test, Explicit("Special local test")]
        public void File_not_exists_subsribe_then_create_it()
        {
            Cleanup();

            var fileName = $"{TestName}_test.tst";
            var source = new JsonFileSource(fileName);
            var settings = new ConfigurationProviderSettings
            {
                ErrorCallBack = e => Console.WriteLine($"ErrorCallBack: {e}"),
            };
            var cp = new ConfigurationProvider(settings)
                .SetupSourceFor<MyClass>(source);

            fileRead = false;
            Console.WriteLine("Subscribing");
            subscription = Subscribe(cp);
            Thread.Sleep(200.Milliseconds());

            Console.WriteLine("Create file");
            TestHelper.CreateFile(TestName, "{'String': 'test', 'Integer': 123}", fileName);
            Thread.Sleep(500.Milliseconds());

            subscription.Dispose();

            fileRead.Should().BeTrue();
        }

        private IDisposable Subscribe(IConfigurationProvider cp) =>
            cp.Observe<MyClass>().SubscribeTo(
                s =>
                {
                    Console.WriteLine($"Read: {JsonSerializer.Serialize(s)}");
                    fileRead = true;
                },
                e =>
                {
                    subscription.Dispose();
                    Thread.Sleep(100.Milliseconds());
                    Console.WriteLine("Resubscription");
                    subscription = Subscribe(cp);
                });

        [ValidateBy(typeof(MyClassValidator))]
        private class MyClass
        {
            public string String { get; set; }
            public int Integer { get; set; }
        }

        private class MyClassValidator: ISettingsValidator<MyClass>
        {
            public void Validate(MyClass value, ISettingsValidationErrors errors)
            {
                if (string.IsNullOrWhiteSpace(value.String))
                    errors.ReportError("String is empty");
                if (value.Integer < 0)
                    errors.ReportError("Int is negative");
            }
        }
    }
}