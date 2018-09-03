using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Validation;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigGeneratorTask_Tests
    {
        private const string ExceptionMessage = "Settings are bad";

        [Test]
        public void Should_create_example()
        {
            var task = new ConfigGeneratorTask { AssemblyPath = Assembly.GetAssembly(GetType()).Location };

            var configFile = typeof(MyConfig).Name;
            var dirPath = Path.Combine(Path.GetDirectoryName(task.AssemblyPath), "settings");
            Directory.Delete(dirPath, true);
            
            var res = false;
            new Action(() => res = task.Execute()).Should().Throw<SettingsValidationException>().WithMessage(ExceptionMessage);
            res.Should().BeFalse();
            var path = Path.Combine(dirPath, configFile + ".example.json");
            var path2 = Path.Combine(dirPath, typeof(MyConfig2).Name + ".example.json");
            File.Exists(path).Should().BeTrue();
            File.Exists(path2).Should().BeTrue();
            File.ReadAllText(path, Encoding.UTF8)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Should()
                .BeEquivalentTo(
                    "{",
                    "\"Int\": 1,",
                    "\"String\": \"str\",",
                    "\"SubConfig\": {",
                    "\"Long\": 1234567890123,",
                    "\"Double\": 1.2345",
                    "}",
                    "}");

            Directory.EnumerateFiles(dirPath).Count().Should().Be(2);
        }

        internal class GoodValidator : ISettingsValidator<MyConfig2>
        {
            public void Validate(MyConfig2 value, ISettingsValidationErrors errors)
            {
            }
        }

        internal class BadValidator : ISettingsValidator<MyConfig3>
        {
            public void Validate(MyConfig3 value, ISettingsValidationErrors errors)
            {
                errors.ReportError(ExceptionMessage);
            }
        }
    }

    [RequiredByDefault]
    public class MyConfig
    {
        public int Int { get; set; } = 1;
        public string String { get; set; } = "str";
        public MySubConfig SubConfig { get; set; } = new MySubConfig();
    }

    public class MySubConfig
    {
        public long Long { get; set; } = 1234567890123L;
        public double Double { get; set; } = 1.2345D;
    }

    [ValidateBy(typeof(ConfigGeneratorTask_Tests.GoodValidator))]
    public class MyConfig2
    {
        public int Int { get; set; } = 1;
        public string String { get; set; } = "str";
    }

    [ValidateBy(typeof(ConfigGeneratorTask_Tests.BadValidator))]
    public class MyConfig3
    {
        public int Int { get; set; } = -1;
    }
}