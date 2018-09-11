using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.Validation;

namespace Vostok.Configuration.Tests
{
    [TestFixture]
    public class ConfigGeneratorTask_Tests
    {
        [Test]
        public void Should_create_json_example()
        {
            var task = new ConfigGeneratorTask
            {
                AssemblyPath = Assembly.GetAssembly(GetType()).Location,
                // SettingsType = "JsOn",   is default
            };

            var configFile1 = typeof(MyConfig).Name;
            var configFile2 = typeof(MyConfig2).Name;
            var dirPath = Path.Combine(Path.GetDirectoryName(task.AssemblyPath), "settings");
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
            
            var res = task.Execute();
            res.Should().BeTrue();
            const string ext = ".example.json";
            var path1 = Path.Combine(dirPath, configFile1 + ext);
            var path2 = Path.Combine(dirPath, configFile2 + ext);
            File.Exists(path1).Should().BeTrue();
            File.Exists(path2).Should().BeTrue();
            File.ReadAllText(path1, Encoding.UTF8)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Should()
                .BeEquivalentTo(
                    "{",
                        "\"Int\": 1,",
                        "\"String\": \"str\",",
                        "\"SubConfig\": {",
                            "\"Long\": 1234567890123,",
                            "\"Double\": 1.2345,",
                            "\"Colors\": [",
                                "0,",
                                "15",
                            "],",
                            $"\"DateTime\": \"{new DateTime(2000, 1, 1):s}\"",
                        "}",
                    "}");
            File.ReadAllText(path2, Encoding.UTF8)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Should()
                .BeEquivalentTo(
                    "{",
                        "\"Int\": 2,",
                        "\"String\": \"string\",",
                        "\"NullString\": null",
                    "}");

            Directory.EnumerateFiles(dirPath).Count().Should().Be(2);
        }

        [Test]
        public void Should_create_ini_example()
        {
            var task = new ConfigGeneratorTask
            {
                AssemblyPath = Assembly.GetAssembly(GetType()).Location,
                ConfigType = "iNi",
            };

            var configFile1 = typeof(MyConfig).Name;
            var configFile2 = typeof(MyConfig2).Name;
            var dirPath = Path.Combine(Path.GetDirectoryName(task.AssemblyPath), "settings");
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);

            var res = task.Execute();
            res.Should().BeTrue();
            const string ext = ".example.ini";
            var path1 = Path.Combine(dirPath, configFile1 + ext);
            var path2 = Path.Combine(dirPath, configFile2 + ext);
            File.Exists(path1).Should().BeTrue();
            File.Exists(path2).Should().BeTrue();
            File.ReadAllText(path1, Encoding.UTF8)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Should()
                .BeEquivalentTo(
                    "Int = 1",
                    "String = str",
                    "SubConfig.Long = 1234567890123",
                    $"SubConfig.Double = {new MySubConfig().Double.ToString(CultureInfo.CurrentCulture)}",
                    "SubConfig.Colors = ",
                    $"SubConfig.DateTime = {new DateTime(2000, 1, 1)}");
            File.ReadAllText(path2, Encoding.UTF8)
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Should()
                .BeEquivalentTo(
                    "Int = 2",
                    "String = string",
                    "NullString = ");

            Directory.EnumerateFiles(dirPath).Count().Should().Be(2);
        }

        [Test]
        public void Should_throw_exception_if_type_is_not_specified()
        {
            var task = new ConfigGeneratorTask
            {
                AssemblyPath = Assembly.GetAssembly(GetType()).Location,
                ConfigType = "wrong_type",
            };

            var dirPath = Path.Combine(Path.GetDirectoryName(task.AssemblyPath), "settings");
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);

            var res = false;
            new Action(() => res = task.Execute()).Should().Throw<ArgumentException>();
            res.Should().BeFalse();
        }

        internal class Validator : ISettingsValidator<MyConfig2>
        {
            public void Validate(MyConfig2 value, ISettingsValidationErrors errors)
            {
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
        public ConsoleColor[] Colors { get; set; } = {ConsoleColor.Black, ConsoleColor.White};
        public DateTime DateTime { get; set; } = new DateTime(2000, 1, 1);
    }

    [ValidateBy(typeof(ConfigGeneratorTask_Tests.Validator))]
    public class MyConfig2
    {
        public int Int { get; set; } = 2;
        public string String { get; set; } = "string";
        public string NullString { get; set; }
    }
}