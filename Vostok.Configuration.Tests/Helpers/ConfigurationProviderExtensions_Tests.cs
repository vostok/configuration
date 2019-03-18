using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Tests.Integration;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class ConfigurationProviderExtensions_Tests : TreeConstructionSet
    {
        private readonly CustomConfig initialConfig = new CustomConfig
        {
            SomeText = "this string is awesome !!1",
            EnableThis = true,
            MaxCount = 9001,
            Timeouts = new TimeoutsConfig
            {
                Get = 123.Seconds(),
                Post = 321.Seconds(),
                Delete = 999.Seconds()
            }
        };
        private readonly CustomConfig updatedConfig = new CustomConfig
        {
            SomeText = "kontur was here >:P",
            EnableThis = false,
            MaxCount = 12345,
            Timeouts = new TimeoutsConfig
            {
                Get = 1.Minutes(),
                Post = 10.Minutes(),
                Delete = 2.Minutes()
            }
        };
        private ConfigurationProvider provider;
        private TestConfigurationSource source;

        public static ISettingsNode CreateObjectNode(CustomConfig config)
        {
            var settings = Object(
                Value("someText", config.SomeText),
                Value("enableThis", config.EnableThis.ToString().ToLowerInvariant()),
                Value("maxCount", config.MaxCount.ToString()));
            if (config.Timeouts == null) return settings;

            var timeouts = Object(
                "timeouts",
                Value("get", config.Timeouts.Get.ToString("c")),
                Value("post", config.Timeouts.Post.ToString("c")),
                Value("delete", config.Timeouts.Delete.ToString("c")));
            return settings.Merge(Object(timeouts));
        }

        public void UpdateConfig(CustomConfig newConfig)
        {
            source.PushNewConfiguration(CreateObjectNode(newConfig));
            Thread.Sleep(200);
        }

        [SetUp]
        public void Setup()
        {
            provider = new ConfigurationProvider();
            source = new TestConfigurationSource();
            UpdateConfig(initialConfig);
        }

        [Test]
        public void Create_hot_should_work_with_interface()
        {
            var config = provider.CreateHot<ICustomConfig>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Create_hot_should_return_hot_config()
        {
            var config = provider.CreateHot<ICustomConfig>(source);
            UpdateConfig(updatedConfig);

            config.Should().BeEquivalentTo(updatedConfig);
        }

        [Test]
        public void Create_hot_should_work_with_setupped_source()
        {
            var customConfig = new CustomConfig {MaxCount = 100500, SomeText = "<null>"};
            provider.SetupSourceFor<ISetuppedConfig>(new TestConfigurationSource(CreateObjectNode(customConfig)));

            var config = provider.CreateHot<ISetuppedConfig>();

            config.Should().BeEquivalentTo(customConfig, options => options.Excluding(c => c.Timeouts));
        }

        [Test]
        public void Create_hot_should_work_with_custom_attributes_of_interface()
        {
            try
            {
                provider.CreateHot<IValidatedConfig>(source);
            }
            catch (TargetInvocationException e)
            {
                var exception = Unwrap(e);
                exception.Should().BeOfType<SettingsValidationException>();
                exception.Message.Should().Contain("testing...");
            }
        }

        [Test]
        public void Create_hot_should_work_with_custom_attributes_of_properties()
        {
            try
            {
                provider.CreateHot<IConfigWithRequiredProperty>(source);
            }
            catch (TargetInvocationException e)
            {
                var exception = Unwrap(e);
                exception.Should().BeOfType<SettingsBindingException>();
                exception.Message.Should().Contain(nameof(IConfigWithRequiredProperty.RequiredTestMessage));
            }
        }

        private static Exception Unwrap(TargetInvocationException exception)
        {
            Exception e = exception;
            while (e.InnerException is TargetInvocationException)
                e = e.InnerException;
            return e.InnerException;
        }
    }

    public class CustomConfig
    {
        public string SomeText { get; set; }
        public bool EnableThis { get; set; }
        public int MaxCount { get; set; }
        public TimeoutsConfig Timeouts { get; set; }
    }

    public class TimeoutsConfig
    {
        public TimeSpan Get { get; set; }
        public TimeSpan Post { get; set; }
        public TimeSpan Delete { get; set; }
    }

    public interface ICustomConfig
    {
        string SomeText { get; }
        bool EnableThis { get; }
        int MaxCount { get; }
        ITimeoutsConfig Timeouts { get; }
    }

    public interface ITimeoutsConfig
    {
        TimeSpan Get { get; }
        TimeSpan Post { get; }
        TimeSpan Delete { get; }
    }

    public interface ISetuppedConfig
    {
        string SomeText { get; }
        bool EnableThis { get; }
        int MaxCount { get; }
    }

    [ValidateBy(typeof(CustomValidator))]
    public interface IValidatedConfig
    {
    }

    public class CustomValidator : ISettingsValidator<IValidatedConfig>
    {
        public IEnumerable<string> Validate(IValidatedConfig settings) => new [] {"testing..."};
    }

    public interface IConfigWithRequiredProperty
    {
        [Required]
        string RequiredTestMessage { get; }
    }
}