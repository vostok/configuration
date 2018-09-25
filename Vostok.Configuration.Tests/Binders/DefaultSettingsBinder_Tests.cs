using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            provider = Substitute.For<ISettingsBinderProvider>();

            binder = new DefaultSettingsBinder(provider);
        }

        [Test]
        public void Should_add_default_parsers_to_given_SettingsBinderProvider()
        {
            provider.Received().SetupParserFor<bool>(Arg.Any<ITypeParser>());
        }

        [Test]
        public void Bind_should_throw_if_provided_settings_is_null()
        {
            new Action(() => binder.Bind<object>(null)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Bind_should_select_appropriate_binder_for_given_type()
        {
            binder.Bind<MyClass>(new ObjectNode(""));

            provider.CreateFor<MyClass>().Received().Bind(Arg.Any<ISettingsNode>());
        }

        [Test]
        public void WithParserFor_should_setup_parser()
        {
            binder.WithParserFor<MyClass>(MyClass.TryParse);

            provider.Received().SetupParserFor<MyClass>(Arg.Any<ITypeParser>());
        }

        [Test]
        public void WithParserFor_should_return_self()
        {
            binder.WithParserFor<MyClass>(MyClass.TryParse).Should().BeSameAs(binder);
        }

        public class MyClass
        {
            public static bool TryParse(string s, out MyClass v)
            {
                v = new MyClass();
                return true;
            }
        }
    }
}