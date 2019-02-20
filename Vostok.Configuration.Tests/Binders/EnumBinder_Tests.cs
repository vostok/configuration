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
            binder.Bind(Value("FirstOption")).UnwrapIfNoErrors().Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_combined_enum_values()
        {
            binder.Bind(Value("FirstOption,SecondOption")).UnwrapIfNoErrors().Should().Be(MyEnum.BothOptions);
        }

        [Test]
        public void Should_support_numeric_values()
        {
            binder.Bind(Value("1")).UnwrapIfNoErrors().Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_support_undefined_values()
        {
            binder.Bind(Value("4")).UnwrapIfNoErrors().Should().Be((MyEnum)4);
        }

        [Test]
        public void Should_be_case_insensitive()
        {
            binder.Bind(Value("firstoption")).UnwrapIfNoErrors().Should().Be(MyEnum.FirstOption);
        }

        [Test]
        public void Should_report_error_for_invalid_values()
        {
            new Action(() => binder.Bind(Value("xxx")).UnwrapIfNoErrors())
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
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