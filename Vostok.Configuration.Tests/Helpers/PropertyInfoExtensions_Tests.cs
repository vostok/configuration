using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class PropertyInfoExtensions_Tests
    {
        [Test]
        public void ForceSetValue_should_work_for_property_with_public_setter()
        {
            var instance = new MyClass();

            instance.GetType().GetProperty(nameof(MyClass.Property1)).ForceSetValue(instance, true);

            instance.Property1.Should().BeTrue();
        }

        [Test]
        public void ForceSetValue_should_work_for_property_with_private_setter()
        {
            var instance = new MyClass();

            instance.GetType().GetProperty(nameof(MyClass.Property2)).ForceSetValue(instance, true);

            instance.Property2.Should().BeTrue();
        }

        [Test]
        public void ForceSetValue_should_work_for_property_without_setter()
        {
            var instance = new MyClass();

            instance.GetType().GetProperty(nameof(MyClass.Property3)).ForceSetValue(instance, true);

            instance.Property3.Should().BeTrue();
        }

        private class MyClass
        {
            public bool Property1 { get; set; }

            public bool Property2 { get; private set; }

            public bool Property3 { get; }
        }
    }
}