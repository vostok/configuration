using System;
using System.Reflection;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ClassStructBinder_Tests : TreeConstructionSet
    {
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISettingsBinder<object>>();
            boolBinder.Bind(Arg.Is<ISettingsNode>(n => n is ValueNode && ((ValueNode)n).Value == "true")).Returns(true);
            boolBinder.ReturnsForAll<object>(_ => throw new SettingsBindingException(""));

            provider = Substitute.For<ISettingsBinderProvider>();
            provider.CreateFor(typeof(bool)).Returns(boolBinder);
        }

        [Test]
        public void Should_set_public_fields()
        {
            var settings = Object(("Field1", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_set_public_properties()
        {
            var settings = Object(("Property1", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_ignore_non_public_fields()
        {
            var settings = Object(("Field2", "true"), ("Field3", "true"), ("Field4", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            GetValue(myClass, "Field2").Should().BeFalse();
            GetValue(myClass, "Field3").Should().BeFalse();
            myClass.Field4.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_non_public_properties()
        {
            var settings = Object(("Property2", "true"), ("Property3", "true"), ("Property4", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            GetValue(myClass, "Property2").Should().BeFalse();
            GetValue(myClass, "Property3").Should().BeFalse();
            myClass.Property4.Should().BeFalse();
        }

        [Test]
        public void Should_set_properties_without_setter()
        {
            var settings = Object(("Property5", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property5.Should().BeTrue();
        }

        [Test]
        public void Should_set_properties_with_non_public_setter()
        {
            var settings = Object(("Property6", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property6.Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = Object(("Field1", "xxx"));

            new Action(() => CreateBinder<MyClass1>().Bind(settings)).Should().Throw<SettingsBindingException>();
        }

        [Test]
        public void Should_throw_if_required_field_is_not_set()
        {
            var settings = Object(("Field1", "true"));

            new Action(() => CreateBinder<MyClass2>().Bind(settings)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_required_property_is_not_set()
        {
            var settings = Object(("Property1", "true"));

            new Action(() => CreateBinder<MyClass2>().Bind(settings)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_required_by_default_field_is_not_set()
        {
            var settings = Object(("Field1", "true"));

            new Action(() => CreateBinder<MyClass3>().Bind(settings)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_if_required_by_default_property_is_not_set()
        {
            var settings = Object(("Property1", "true"));

            new Action(() => CreateBinder<MyClass3>().Bind(settings)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_keep_default_value_of_field()
        {
            var settings = new ObjectNode(null as string);

            var myClass = CreateBinder<MyClass4>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_keep_default_value_of_property()
        {
            var settings = new ObjectNode(null as string);

            var myClass = CreateBinder<MyClass4>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_type_has_no_parameterless_constructor()
        {
            var settings = Object(("Field1", "true"));

            new Action(() => CreateBinder<MyClass5>().Bind(settings)).Should().Throw<MissingMethodException>(); // TODO(krait): wrap exception
        }

        [Test]
        public void Should_set_multiple_properties_of_struct()
        {
            var settings = Object(("Property1", "true"), ("Property2", "true"));

            var myStruct = CreateBinder<MyStruct>().Bind(settings);

            myStruct.Property1.Should().BeTrue();
            myStruct.Property2.Should().BeTrue();
        }

        [Test]
        public void Should_set_members_to_default_values_if_node_is_null_and_all_members_are_optional()
        {
            var myClass = CreateBinder<MyClass6>().Bind(null);

            myClass.Property1.Should().BeTrue();
            myClass.Field1.Should().BeTrue();
            myClass.Field2.Should().NotBeNull().And.BeOfType<MyClass6.Inner>();
        }
        
        [Test]
        public void Should_throw_if_node_is_null_and_some_of_members_are_required()
        {
            new Action(() => CreateBinder<MyClass7>().Bind(null)).Should().Throw<SettingsBindingException>();
        }

        private class MyClass1
        {
            public bool Field1;
            private bool Field2;
            protected bool Field3;
            internal bool Field4;

            public bool Property1 { get; set; }
            private bool Property2 { get; set; }
            protected bool Property3 { get; set; }
            internal bool Property4 { get; set; }
            public bool Property5 { get; }
            public bool Property6 { get; private set; }
        }

        private class MyClass2
        {
            [Required]
            public bool Field1;

            [Required]
            public bool Property1;
        }

        [RequiredByDefault]
        private class MyClass3
        {
            public bool Field1;
            public bool Property1 { get; set; }
        }

        private class MyClass4
        {
            public bool Field1 = true;
            public bool Property1 { get; set; } = true;
        }

        private class MyClass5
        {
            public MyClass5(bool field1) => Field1 = field1;

            public bool Field1;
        }
        
        private class MyClass6
        {
            public bool Field1 = true;
            public Inner Field2 = new Inner();
            public bool Property1 { get; set; } = true;
            
            public class Inner
            {
                public bool Field;
            }
        }
        
        private class MyClass7
        {
            public bool Property1 { get; set; }
            
            [Required]
            public bool Property2 { get; set; }
        }

        private struct MyStruct
        {
            public bool Property1 { get; set; }
            public bool Property2 { get; set; }
        }

        private static bool? GetValue(object obj, string fieldName)
        {
            var field = obj?.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var property = obj?.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            return obj == null ? null : (field?.GetValue(obj) as bool? ?? property?.GetValue(obj) as bool?);
        }
        
        private ClassStructBinder<T> CreateBinder<T>() => new ClassStructBinder<T>(provider);
    }
}