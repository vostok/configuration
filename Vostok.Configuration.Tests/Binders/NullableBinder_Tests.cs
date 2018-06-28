using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class NullableBinder_Tests : Binders_Test
    {
        [Test]
        public void Should_bind_to_Nullable_Bool()
        {
            var settings = new ValueNode("true");
            var binder = Container.GetInstance<ISettingsBinder<bool?>>();
            binder.Bind(settings).Should().Be(true);

            settings = new ValueNode(null, "");
            binder = Container.GetInstance<ISettingsBinder<bool?>>();
            binder.Bind(settings).Should().Be(null);
        }

        [Test]
        public void Should_bind_to_Nullable_Double()
        {
            var settings = new ValueNode("1.2345");
            var binder = Container.GetInstance<ISettingsBinder<double?>>();
            binder.Bind(settings).Should().Be(1.2345);

            settings = new ValueNode(null, "");
            binder = Container.GetInstance<ISettingsBinder<double?>>();
            binder.Bind(settings).Should().Be(null);
        }

        [Test]
        public void Should_throw_exception_if_tree_is_null()
        {
            var binder = Container.GetInstance<ISettingsBinder<bool?>>();
            new Action(() => binder.Bind(null)).Should().Throw<ArgumentNullException>();
        }
    }
}