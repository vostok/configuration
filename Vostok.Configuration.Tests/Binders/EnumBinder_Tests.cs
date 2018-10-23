using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class EnumBinder_Tests
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
            binder.Bind(new ValueNode("FirstOption")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_combined_enum_values()
        {
            binder.Bind(new ValueNode("FirstOption,SecondOption")).Should().Be(MyEnum.BothOptions);
        }

        [Test]
        public void Should_support_numeric_values()
        {
            binder.Bind(new ValueNode("1")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_undefined_values() // TODO(krait): Should it?
        {
            binder.Bind(new ValueNode("4")).Should().Be((MyEnum)4);
        }

        [Test]
        public void Should_be_case_insensitive()
        {
            binder.Bind(new ValueNode("firstoption")).Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_throw_for_invalid_values()
        {
            new Action(() => binder.Bind(new ValueNode("xxx"))).Should().Throw<BindingException>().Which.ShouldBePrinted();
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