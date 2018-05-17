using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SimpleInjector;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class EnumBinder_Tests
    {
        private Container container;

        [SetUp]
        public void SetUp()
        {
            container = new Container();
            container.Register(typeof(ISettingsBinder<>), typeof(PrimitiveAndSimpleBinder));
            container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            container.Register(typeof(ISettingsBinder<>), typeof(EnumBinder<>));
        }

        [Test]
        public void Should_bind_to_Enum_by_value_or_name()
        {
            var settings = new RawSettings("10");
            var binder = container.GetInstance<ISettingsBinder<ConsoleColor>>();
            binder.Bind(settings).Should().Be(ConsoleColor.Green);

            settings = new RawSettings("grEEn");
            binder.Bind(settings).Should().Be(ConsoleColor.Green);
        }

        [Test]
        public void Throw_exception_if_cannot_get_value()
        {
            var settings = new RawSettings("зелёный");
            var binder = container.GetInstance<ISettingsBinder<ConsoleColor>>();
            new Action(() => binder.Bind(settings)).Should().Throw<InvalidCastException>();
        }
    }
}