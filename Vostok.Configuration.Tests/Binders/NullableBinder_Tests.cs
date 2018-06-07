using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class NullableBinder_Tests : Binders_Test
    {
        [Test]
        public void Should_bind_to_Nullable_Bool()
        {
            var settings = new SettingsNode("true");
            var binder = Container.GetInstance<ISettingsBinder<bool?>>();
            binder.Bind(settings).Should().Be(true);

            settings = new SettingsNode(null, "");
            binder = Container.GetInstance<ISettingsBinder<bool?>>();
            binder.Bind(settings).Should().Be(null);
        }
        [Test]
        public void Should_bind_to_Nullable_Double()
        {
            var settings = new SettingsNode("1.2345");
            var binder = Container.GetInstance<ISettingsBinder<double?>>();
            binder.Bind(settings).Should().Be(1.2345);

            settings = new SettingsNode(null, "");
            binder = Container.GetInstance<ISettingsBinder<double?>>();
            binder.Bind(settings).Should().Be(null);
        }
    }
}