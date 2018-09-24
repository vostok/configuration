using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class ClassAndStructBinder_Tests
    {
        private ISettingsBinderFactory factory;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISettingsBinder<object>>();
            boolBinder.Bind(Arg.Is<ISettingsNode>(n => n is ValueNode && ((ValueNode)n).Value == "true")).Returns(true);
            boolBinder.ReturnsForAll<object>(_ => throw new InvalidCastException());

            factory = Substitute.For<ISettingsBinderFactory>();
            factory.CreateFor(typeof(bool)).Returns(boolBinder);
        }

        [Test]
        public void Should_set_public_fields()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field1", new ValueNode("true") }
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_set_public_properties()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property1", new ValueNode("true") }
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_ignore_non_public_fields()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field2", new ValueNode("true") },
                { "Field3", new ValueNode("true") },
                { "Field4", new ValueNode("true") },
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            GetValue(myClass, "Field2").Should().BeFalse();
            GetValue(myClass, "Field3").Should().BeFalse();
            myClass.Field4.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_non_public_properties()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property2", new ValueNode("true") },
                { "Property3", new ValueNode("true") },
                { "Property4", new ValueNode("true") },
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            GetValue(myClass, "Property2").Should().BeFalse();
            GetValue(myClass, "Property3").Should().BeFalse();
            myClass.Property4.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_readonly_properties()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property5", new ValueNode("true") }
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property5.Should().BeFalse();
        }


        [Test]
        public void Should_set_properties_with_non_public_setter()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property6", new ValueNode("true") }
            });

            var myClass = CreateBinder<MyClass1>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property6.Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_inner_binder_throws()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field1", new ValueNode("xxx") }
            });

            new Action(() => CreateBinder<MyClass1>().Bind(settings)).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_if_required_field_is_not_set()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property1", new ValueNode("true") }
            });

            new Action(() => CreateBinder<MyClass2>().Bind(settings)).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_if_required_property_is_not_set()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field1", new ValueNode("true") }
            });

            new Action(() => CreateBinder<MyClass2>().Bind(settings)).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_if_required_by_default_field_is_not_set()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property1", new ValueNode("true") }
            });

            new Action(() => CreateBinder<MyClass3>().Bind(settings)).Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Should_throw_if_required_by_default_property_is_not_set()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field1", new ValueNode("true") }
            });

            new Action(() => CreateBinder<MyClass3>().Bind(settings)).Should().Throw<InvalidCastException>(); // TODO(krait): choose another exception
        }

        [Test]
        public void Should_keep_default_value_of_field()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>());

            var myClass = CreateBinder<MyClass4>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_keep_default_value_of_property()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>());

            var myClass = CreateBinder<MyClass4>().Bind(settings);

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_throw_if_type_has_no_parameterless_constructor()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Field1", new ValueNode("true") }
            });

            new Action(() => CreateBinder<MyClass5>().Bind(settings)).Should().Throw<MissingMethodException>(); // TODO(krait): wrap exception
        }

        [Test]
        public void Should_set_multiple_properties_of_struct()
        {
            var settings = new ObjectNode(new Dictionary<string, ISettingsNode>
            {
                { "Property1", new ValueNode("true") },
                { "Property2", new ValueNode("true") }
            });

            var myStruct = CreateBinder<MyStruct>().Bind(settings);

            myStruct.Property1.Should().BeTrue();
            myStruct.Property2.Should().BeTrue();
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
            public bool Property1;
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
        
        private ClassAndStructBinder<T> CreateBinder<T>() => new ClassAndStructBinder<T>(factory);
    }
}