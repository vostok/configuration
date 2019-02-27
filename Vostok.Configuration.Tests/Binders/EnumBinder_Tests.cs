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
            binder.Bind(Value("FirstOption")).Value.Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_combined_enum_values()
        {
            binder.Bind(Value("FirstOption,SecondOption")).Value.Should().Be(MyEnum.BothOptions);
        }

        [Test]
        public void Should_support_numeric_values()
        {
            binder.Bind(Value("1")).Value.Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_undefined_values()
        {
            binder.Bind(Value("4")).Value.Should().Be((MyEnum)4);
        }

        [Test]
        public void Should_be_case_insensitive()
        {
            binder.Bind(Value("firstoption")).Value.Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_report_error_for_invalid_values()
        {
            new Func<MyEnum>(() => binder.Bind(Value("xxx")).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_default_value_for_missing_nodes()
        {
            binder.Bind(null).Value.Should().Be(default(MyEnum));
        }

        [Test]
        public void Should_return_default_value_for_null_value_nodes()
        {
            binder.Bind(Value(null)).Value.Should().Be(default(MyEnum));
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