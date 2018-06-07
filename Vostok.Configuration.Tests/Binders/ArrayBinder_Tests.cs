using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ArrayBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Array_of_primitives()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                ["1"] = new SettingsNode("TRUE"),
                ["2"] = new SettingsNode("false"),
            });
            var binder = Container.GetInstance<ISettingsBinder<bool[]>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[] { true, false });
        }

        [Test]
        public void Should_bind_to_Array_of_structs()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                ["1"] = new SettingsNode(new OrderedDictionary
                {
                    { "Int", new SettingsNode("1") },
                    { "String", new SettingsNode("str1") },
                }),
                ["2"] = new SettingsNode(new OrderedDictionary
                {
                    { "Int", new SettingsNode("2") },
                    { "String", new SettingsNode("str2") },
                }),
            });
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct[]>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new[]
            {
                new SimpleStruct{ Int = 1, String = "str1" },
                new SimpleStruct{ Int = 2, String = "str2" },
            });
        }

        private struct SimpleStruct
        {
            public int Int { get; set; }
            public string String { get; set; }
        }
    }
}