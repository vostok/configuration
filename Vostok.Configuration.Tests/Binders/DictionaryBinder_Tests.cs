using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class DictionaryBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Dictionary_of_primitives()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                { "1", new SettingsNode("10", "1") },
                { "2", new SettingsNode("20", "2") },
            });
            var binder = Container.GetInstance<ISettingsBinder<Dictionary<int,int>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, int> { { 1, 10 }, { 2, 20 } });
        }

        [Test]
        public void Should_bind_to_IDictionary_of_primitives()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                { "1", new SettingsNode("1.23", "1") },
                { "2", new SettingsNode("2.34", "2") },
            });
            var binder = Container.GetInstance<ISettingsBinder<IDictionary<int, double>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, double> { { 1, 1.23 }, { 2, 2.34 } });
        }

        [Test]
        public void Should_bind_to_IReadOnlyDictionary_of_primitives()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                { "true", new SettingsNode("FALSE", "true") },
                { "false", new SettingsNode("TRUE", "false") },
            });
            var binder = Container.GetInstance<ISettingsBinder<IReadOnlyDictionary<bool, bool>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<bool, bool> { { true, false }, { false, true } });
        }

        [Test]
        public void Should_bind_to_dictionary_of_dictionaries_of_primitives()
        {
            var settings = new SettingsNode(new OrderedDictionary
            {
                { "1", new SettingsNode(new OrderedDictionary
                {
                    { "100", new SettingsNode("true", "100") },
                }, "1") },
                { "2", new SettingsNode(new OrderedDictionary
                {
                    { "200", new SettingsNode("false", "200") },
                }, "2") },
            });
            var binder = Container.GetInstance<ISettingsBinder<Dictionary<int, Dictionary<long, bool>>>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new Dictionary<int, Dictionary<long, bool>>
            {
                { 1, new Dictionary<long, bool>
                {
                    { 100, true },
                } },
                { 2, new Dictionary<long, bool>
                {
                    { 200, false },
                } }
            });
        }
    }
}