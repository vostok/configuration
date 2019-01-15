using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class EnumBinder_Tests : TreeConstructionSet
    {
        private EnumBinder<MyEnum> binder;

        [SetUp]
        public void TestSetup()
        {
            binder = new EnumBinder<MyEnum>();
        }

        [Test]
        public void Should_support_single_enum_values()
        {
            binder.Bind(Value("FirstOption")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_combined_enum_values()
        {
            binder.Bind(Value("FirstOption,SecondOption")).Should().Be(MyEnum.BothOptions);
        }

        [Test]
        public void Should_support_numeric_values()
        {
            binder.Bind(Value("1")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_undefined_values() // TODO(krait): Should it?
        {
            binder.Bind(Value("4")).Should().Be((MyEnum)4);
        }

        [Test]
        public void Should_support_null_settings_node()
        {
            binder.Bind(null).Should().Be(default(MyEnum));
        }

        [Test]
        public void Should_be_case_insensitive()
        {
            binder.Bind(Value("firstoption")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_throw_for_invalid_values()
        {
            new Action(() => binder.Bind(Value("xxx"))).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Flags]
        private enum MyEnum
        {
            FirstOption = 1,
            SecondOption = 2,
            BothOptions = 3
        }
    }
}