using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
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

        public static ISettingsNode CreateObjectNode(CustomConfig config) => Object(
            Value("someText", config.SomeText),
            Value("enableThis", config.EnableThis.ToString().ToLowerInvariant()),
            Value("maxCount", config.MaxCount.ToString()),
            Object(
                "timeouts",
                Value("get", config.Timeouts.Get.ToString("c")),
                Value("post", config.Timeouts.Post.ToString("c")),
                Value("delete", config.Timeouts.Delete.ToString("c"))));

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
        public void Get_hot_should_work_with_interface()
        {
            var (config, _) = provider.GetHot<ICustomConfig>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Get_hot_should_return_hot_config()
        {
            var (config, _) = provider.GetHot<ICustomConfig>(source);
            UpdateConfig(updatedConfig);

            config.Should().BeEquivalentTo(updatedConfig);
        }

        [Test]
        public void Get_hot_should_work_with_internal_interface()
        {
            var (config, _) = provider.GetHot<IInternalCustomConfig>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Get_hot_should_return_hot_config_with_internal_interface()
        {
            var (config, _) = provider.GetHot<IInternalCustomConfig>(source);
            UpdateConfig(updatedConfig);

            config.Should().BeEquivalentTo(updatedConfig);
        }

        [Test]
        public void Get_hot_should_ignore_methods_in_interface()
        {
            var (config, _) = provider.GetHot<ICustomConfig>(source);

            ((Action)config.DoSmthng).Should().Throw<NotImplementedException>();
        }
    }

    public interface ICustomConfig
    {
        string SomeText { get; }
        bool EnableThis { get; }
        int MaxCount { get; }
        ITimeoutsConfig Timeouts { get; }
        void DoSmthng();
    }

    public interface ITimeoutsConfig
    {
        TimeSpan Get { get; }
        TimeSpan Post { get; }
        TimeSpan Delete { get; }
    }

    internal interface IInternalCustomConfig
    {
        string SomeText { get; }
        bool EnableThis { get; }
        int MaxCount { get; }
        IInternalTimeoutsConfig Timeouts { get; }
        void DoSmthng();
    }

    internal interface IInternalTimeoutsConfig
    {
        TimeSpan Get { get; }
        TimeSpan Post { get; }
        TimeSpan Delete { get; }
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
}