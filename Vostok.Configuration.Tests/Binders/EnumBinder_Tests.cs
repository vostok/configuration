using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class EnumBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Enum_by_value_or_name()
        {
            var settings = new SettingsNode("10");
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            binder.Bind(settings).Should().Be(ConsoleColor.Green);

            settings = new SettingsNode("grEEn");
            binder.Bind(settings).Should().Be(ConsoleColor.Green);
        }

        [Test]
        public void Throw_exception_if_cannot_get_value()
        {
            var settings = new SettingsNode("зелёный");
            var binder = Container.GetInstance<ISettingsBinder<ConsoleColor>>();
            new Action(() => binder.Bind(settings)).Should().Throw<InvalidCastException>();
        }
    }
}