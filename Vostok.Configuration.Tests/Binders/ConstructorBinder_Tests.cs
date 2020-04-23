using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Tests.Binders
{
    public class ConstructorBinder_Tests : TreeConstructionSet
    {
        private ISettingsBinderProvider provider;
        private ISettingsNode settings;

        [SetUp]
        public void SetUp()
        {
            var boolBinder = Substitute.For<ISafeSettingsBinder<object>>();
            boolBinder.Bind(Arg.Is<ISettingsNode>(n => n is ValueNode && ((ValueNode)n).Value == "true"))
                .Returns(SettingsBindingResult.Success<object>(true));
            boolBinder.ReturnsForAll(_ => SettingsBindingResult.Error<object>(":("));

            provider = Substitute.For<ISettingsBinderProvider>();
            provider.CreateFor(typeof(bool)).Returns(boolBinder);

            settings = new ValueNode("true");
        }

        [Test]
        public void Should_use_constructor()
        {
            var binder = new ConstructorBinder<MyClass>(provider);

            var result = binder.Bind(settings);

            result.Errors.Should().BeEmpty();
            result.Value.GetValue().Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_type_has_no_appropriate_constructor()
        {
            new Func<MyClass2>(() => new ConstructorBinder<MyClass2>(provider).Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_type_has_no_appropriate_constructor_with_matched_type()
        {
            new Func<MyClass3>(() => new ConstructorBinder<MyClass3>(provider).Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_type_has_multiple_appropriate_constructors()
        {
            new Func<MyClass4>(() => new ConstructorBinder<MyClass4>(provider).Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        private class MyClass
        {
            private readonly bool value;

            public MyClass(bool value)
            {
                this.value = value;
            }
            
            public bool GetValue() => value;
        }

        private class MyClass2
        {
        }

        private class MyClass3
        {
            public MyClass3(int value)
            {
            }
        }

        private class MyClass4
        {
            public MyClass4(int value)
            {
            }

            public MyClass4(string value)
            {
            }
        }
    }
}