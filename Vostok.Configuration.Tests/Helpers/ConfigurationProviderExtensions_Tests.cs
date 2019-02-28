using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions.ConfigurationProvider;
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
            var config = provider.GetHot<ICustomConfig>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Get_hot_should_work_with_normal_class()
        {
            var config = provider.GetHot<CustomConfig>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Get_hot_should_work_with_normal_class_with_fields()
        {
            var config = provider.GetHot<CustomConfigWithFields>(source);

            config.Should().BeEquivalentTo(initialConfig);
        }

        [Test]
        public void Get_hot_should_not_work_with_abstract_class()
        {
            var action = (Action)(() => provider.GetHot<CustomConfigBase>(source));

            action.Should().Throw<Exception>();
        }

        [Test]
        public void Get_hot_should_not_work_with_value_types()
        {
            var action = (Action)(() => provider.GetHot<bool>(source));

            action.Should().Throw<Exception>();
        }

        [Test]
        public void Get_hot_should_ignore_methods_in_interface()
        {
            var config = provider.GetHot<ICustomConfig>(source);

            ((Action)config.DoSmthng).Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Get_hot_should_return_hot_config()
        {
            var config = provider.GetHot<ICustomConfig>(source);
            UpdateConfig(updatedConfig);

            config.Should().BeEquivalentTo(updatedConfig);
        }

        [Test]
        public void Get_hot_should_return_equal_configs()
        {
            var configA = provider.GetHot<ICustomConfig>(source);
            var configB = provider.GetHot<ICustomConfig>(source);

            ReferenceEquals(configA, configB).Should().BeTrue();
        }

        [Test]
        public void Propertiy_with_interface_type_should_be_hot()
        {
            var timeouts = provider.GetHot<ICustomConfig>(source).Timeouts;
            UpdateConfig(updatedConfig);

            timeouts.Should().BeEquivalentTo(updatedConfig.Timeouts);
        }

        [Test]
        public void Field_with_interface_type_should_be_hot()
        {
            var timeouts = provider.GetHot<CustomConfigWithFields>(source).Timeouts;
            UpdateConfig(updatedConfig);

            timeouts.Should().BeEquivalentTo(updatedConfig.Timeouts);
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

    public class CustomConfig
    {
        public string SomeText { get; set; }
        public bool EnableThis { get; set; }
        public int MaxCount { get; set; }
        public TimeoutsConfig Timeouts { get; set; }
    }

    public class CustomConfigWithFields
    {
        public string SomeText;
        public bool EnableThis;
        public int MaxCount;
        public ITimeoutsConfig Timeouts;
    }

    public abstract class CustomConfigBase
    {
        public abstract string SomeText { get; set; }
        public abstract bool EnableThis { get; set; }
        public abstract int MaxCount { get; set; }
        public abstract TimeoutsConfig Timeouts { get; set; }
    }

    public interface ITimeoutsConfig
    {
        TimeSpan Get { get; }
        TimeSpan Post { get; }
        TimeSpan Delete { get; }
    }

    public class TimeoutsConfig
    {
        public TimeSpan Get { get; set; }
        public TimeSpan Post { get; set; }
        public TimeSpan Delete { get; set; }
    }
}