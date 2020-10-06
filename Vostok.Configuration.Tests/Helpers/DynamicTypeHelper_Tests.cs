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
        public void Helper_should_implement_generic_interface_with_different_types_multiple_times()
        {
            var implType1 = DynamicTypesHelper.ImplementType(typeof(IGenericInterface<bool, int>));
            var implType2 = DynamicTypesHelper.ImplementType(typeof(IGenericInterface<int, bool>));

            implType1.GetInterfaces().Should().Contain(typeof(IGenericInterface<bool, int>));
            implType2.GetInterfaces().Should().Contain(typeof(IGenericInterface<int, bool>));
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

        [Test]
        public void Helper_should_add_attributes_with_array_arguments()
        {
            var expectation = new CustomAttribute(new[] {typeof(bool), typeof(AttributeTargets)}, true, AttributeTargets.All)
            {
                Strings = new[] {"asd", "xyz"},
                Numbers = new[] {1, 2, 3, 4, 5}
            };

            var implType = DynamicTypesHelper.ImplementType(typeof(ITestArrayInAttribute));

            var attribute = implType.GetCustomAttribute<CustomAttribute>();
            attribute.Should().BeEquivalentTo(expectation);
        }
    }

    public interface IInterfaceWithMethod
    {
        void DoSomething();
    }

    internal interface IInternalInterface
    {
    }

    public interface IGenericInterface<T1, T2>
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

    [Custom(new[] {typeof(bool), typeof(AttributeTargets)}, true, AttributeTargets.All, Strings = new[] {"asd", "xyz"}, Numbers = new[] {1, 2, 3, 4, 5})]
    public interface ITestArrayInAttribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        public string Name;
        public int Number { get; set; }
        public Type Type { get; }
        public string[] Strings { get; set; }
        public int[] Numbers { get; set; }
        public object[] Data { get; }

        public CustomAttribute(Type type = null) => Type = type;

        public CustomAttribute(params object[] data) => Data = data;
    }
}