using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    public class DynamicTypeHelper_Tests
    {
        [Test]
        public void Helper_should_ignore_methods()
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(IInterfaceWithMethod));

            var instance = (IInterfaceWithMethod)Activator.CreateInstance(implType);
            ((Action)instance.DoSomething).Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Helper_should_implement_internal_interface()
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(IInternalInterface));

            implType.GetInterfaces().Should().Contain(typeof(IInternalInterface));
        }

        [Test]
        public void Helper_should_add_attributes_to_type()
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(ICustomInterface));

            var attributes = implType.GetCustomAttributes<CustomAttribute>().ToArray();
            attributes.Should().NotBeEmpty();
            attributes[0].Should().BeEquivalentTo(new CustomAttribute(typeof(ICustomInterface)) {Number = 42, Name = "test"});
            attributes[1].Should().BeEquivalentTo(new CustomAttribute(typeof(CustomAttribute)));
        }

        [Test]
        public void Helper_should_add_attributes_to_property()
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(ICustomInterface));
            var info = implType.GetProperty(nameof(ICustomInterface.Number));

            info.GetCustomAttribute<CustomAttribute>().Should().NotBeNull();
        }

        [Test]
        public void Helper_should_add_attributes_to_getter_and_setter()
        {
            var implType = DynamicTypesHelper.ImplementType(typeof(ICustomInterface));
            var info = implType.GetProperty(nameof(ICustomInterface.Number));

            info.GetMethod.GetCustomAttribute<CustomAttribute>().Should().NotBeNull();
            info.SetMethod.GetCustomAttribute<CustomAttribute>().Should().NotBeNull();
        }
    }

    public interface IInterfaceWithMethod
    {
        void DoSomething();
    }

    internal interface IInternalInterface
    {
    }

    [Custom(typeof(ICustomInterface), Number = 42, Name = "test")]
    [Custom(typeof(CustomAttribute))]
    public interface ICustomInterface
    {
        [Custom]
        int Number
        {
            [Custom]
            get;
            [Custom]
            set;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        public string Name;
        public int Number { get; set; }
        public Type Type { get; }

        public CustomAttribute(Type type = null) => Type = type;
    }
}