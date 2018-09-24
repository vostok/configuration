using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class EnumBinder_Tests: Binders_Test
    {
        [TestCase("10")]
        [TestCase("grEEn")]
        public void Should_bind_to_Enum_by_value(string value)
        {
            var settings = new ValueNode(value);
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            binder.Bind(settings).Should().Be(ConsoleColor.Green);
        }

        [Test]
        public void Throw_exception_if_cannot_get_value()
        {
            var settings = new ValueNode("зелёный");
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            new Action(() => binder.Bind(settings)).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_exception_if_tree_is_null()
        {
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            new Action(() => binder.Bind(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_exception_if_tree_is_empty()
        {
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            new Action(() => binder.Bind(new ValueNode(null))).Should().Throw<ArgumentNullException>();
        }
    }
}