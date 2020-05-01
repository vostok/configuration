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
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Tests.Binders
{
    public class ClassStructBinder_Tests : TreeConstructionSet
    {
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            var boolBinder = Substitute.For<ISafeSettingsBinder<object>>();
            boolBinder.Bind(Arg.Is<ISettingsNode>(n => n is ValueNode && ((ValueNode)n).Value == "true"))
                .Returns(SettingsBindingResult.Success<object>(true));
            boolBinder.ReturnsForAll(_ => SettingsBindingResult.Error<object>(":("));

            provider = Substitute.For<ISettingsBinderProvider>();
            provider.CreateFor(typeof(bool)).Returns(boolBinder);
        }

        [Test]
        public void Should_set_public_fields()
        {
            var settings = Object(("Field1", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_set_public_properties()
        {
            var settings = Object(("Property1", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_ignore_non_public_fields()
        {
            var settings = Object(("Field2", "true"), ("Field3", "true"), ("Field4", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            GetValue(myClass, "Field2").Should().BeFalse();
            GetValue(myClass, "Field3").Should().BeFalse();
            myClass.Field4.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_non_public_properties()
        {
            var settings = Object(("Property2", "true"), ("Property3", "true"), ("Property4", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            GetValue(myClass, "Property2").Should().BeFalse();
            GetValue(myClass, "Property3").Should().BeFalse();
            myClass.Property4.Should().BeFalse();
        }

        [Test]
        public void Should_set_properties_without_setter()
        {
            var settings = Object(("Property5", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Property5.Should().BeTrue();
        }

        [Test]
        public void Should_set_properties_with_non_public_setter()
        {
            var settings = Object(("Property6", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Property6.Should().BeTrue();
        }

        [Test]
        public void Should_ignore_static_properties()
        {
            var settings = Object(("StaticProperty", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            MyClass1.StaticProperty.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_static_fields()
        {
            var settings = Object(("StaticField", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            MyClass1.StaticField.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_constants()
        {
            var settings = Object(("Const", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            MyClass1.Const.Should().BeFalse();
        }

        [Test]
        public void Should_ignore_computed_properties()
        {
            var settings = Object(("ComputedProperty", "true"));

            var myClass = CreateBinder<MyClass1>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.ComputedProperty.Should().BeFalse();
        }

        [Test]
        public void Should_report_errors_from_inner_binder()
        {
            var settings = Object(("Field1", "xxx"));

            new Func<MyClass1>(() => CreateBinder<MyClass1>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_report_error_if_required_field_is_not_set()
        {
            var settings = Object(("Property1", "true"));

            new Func<MyClass2>(() => CreateBinder<MyClass2>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_report_error_if_required_property_is_not_set()
        {
            var settings = Object(("Field1", "true"));

            new Func<MyClass2>(() => CreateBinder<MyClass2>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_report_error_if_required_by_default_field_is_not_set()
        {
            var settings = Object(("Property1", "true"));

            new Func<MyClass3>(() => CreateBinder<MyClass3>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_report_error_if_required_by_default_property_is_not_set()
        {
            var settings = Object(("Field1", "true"));

            new Func<MyClass3>(() => CreateBinder<MyClass3>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_keep_default_value_of_missing_field()
        {
            var settings = new ObjectNode(null as string);

            var myClass = CreateBinder<MyClass4>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeTrue();
        }

        [Test]
        public void Should_keep_default_value_of_missing_property()
        {
            var settings = new ObjectNode(null as string);

            var myClass = CreateBinder<MyClass4>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_set_null_value_field_to_default()
        {
            var settings = Object(("Field1", null));

            var myClass = CreateBinder<MyClass4>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Field1.Should().BeFalse();
        }

        [Test]
        public void Should_set_null_value_property_to_default()
        {
            var settings = Object(("Property1", null));

            var myClass = CreateBinder<MyClass4>().Bind(settings).Value;

            myClass.Should().NotBeNull();
            myClass.Property1.Should().BeFalse();
        }

        [Test]
        public void Should_throw_if_type_has_no_parameterless_constructor()
        {
            var settings = Object(("Field1", "true"));

            new Func<MyClass5>(() => CreateBinder<MyClass5>().Bind(settings).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_set_multiple_properties_of_struct()
        {
            var settings = Object(("Property1", "true"), ("Property2", "true"));

            var myStruct = CreateBinder<MyStruct>().Bind(settings).Value;

            myStruct.Property1.Should().BeTrue();
            myStruct.Property2.Should().BeTrue();
        }

        [Test]
        public void Should_return_object_with_default_values_for_missing_nodes()
        {
            CreateBinder<MyClass4>().Bind(null).Value.Field1.Should().BeTrue();
            CreateBinder<MyClass4>().Bind(null).Value.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_return_object_with_default_values_for_null_value_nodes()
        {
            CreateBinder<MyClass4>().Bind(Value(null)).Value.Field1.Should().BeTrue();
            CreateBinder<MyClass4>().Bind(Value(null)).Value.Property1.Should().BeTrue();
        }

        [Test]
        public void Should_return_error_if_node_is_missing_and_some_fields_are_required()
        {
            new Func<MyClass2>(() => CreateBinder<MyClass2>().Bind(null).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
            new Func<MyClass3>(() => CreateBinder<MyClass3>().Bind(null).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_error_if_node_is_null_value_and_some_fields_are_required()
        {
            new Func<MyClass2>(() => CreateBinder<MyClass2>().Bind(Value(null)).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
            new Func<MyClass3>(() => CreateBinder<MyClass3>().Bind(Value(null)).Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_treat_null_literal_as_null_value_for_ref_types()
        {
            Value("null").IsNullValue(CreateBinder<MyClass1>()).Should().BeTrue();
            Value("NULL").IsNullValue(CreateBinder<MyClass1>()).Should().BeTrue();
        }

        [Test]
        public void Should_not_treat_null_literal_as_null_value_for_value_types()
        {
            Value("null").IsNullValue(CreateBinder<MyStruct>()).Should().BeFalse();
            Value("NULL").IsNullValue(CreateBinder<MyStruct>()).Should().BeFalse();
        }

        [Test]
        public void Should_support_BindBy_attribute_on_fields()
        {
            var settings = Object(Value("FieldWithBinder", "String"));

            CreateBinder<MyClass8>().Bind(settings).Value.FieldWithBinder.Should().Be("MyString");
        }

        [Test]
        public void Should_support_BindBy_attribute_on_properties()
        {
            var settings = Object(Value("PropertyWithBinder", "String"));

            CreateBinder<MyClass8>().Bind(settings).Value.PropertyWithBinder.Should().Be("MyString");
        }

        [TestCase("property.name.1")]
        [TestCase("42_property_name")]
        [TestCase("$prop")]
        public void Should_bind_by_alias_if_some_were_set(string alias)
        {
            var settings = Object(Value(alias, "true"));

            CreateBinder<MyClass9>().Bind(settings).Value.PropertyWithAlias.Should().BeTrue();
        }

        [Test]
        public void Should_select_distinct_alias_names_without_case_sensitivity()
        {
            CreateBinder<MyClass10>().Bind(Object(Value("alias", "true"))).Value.PropertyWithAlias.Should().BeTrue();
            CreateBinder<MyClass10>().Bind(Object(Value("propertywithalias", "true"))).Value.PropertyWithAlias.Should().BeTrue();
        }

        [Test]
        public void Should_not_allow_ambiguity_if_there_are_several_matching_aliases()
        {
            var settings = Object(Value("property.name.1", "true"), Value(nameof(MyClass9.PropertyWithAlias), "true"));

            var settingsBindingResult = CreateBinder<MyClass9>().Bind(settings);

            settingsBindingResult
                .Invoking(r => _ = r.Value)
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
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

            public static bool StaticProperty { get; set; }
            public static bool StaticField;
            public const bool Const = false;
            public string this[string index] => throw new NotSupportedException();
            public bool ComputedProperty => false;
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

        private class MyClass8
        {
            [BindBy(typeof(MyStringBinder))]
            public string FieldWithBinder;

            [BindBy(typeof(MyStringBinder))]
            public string PropertyWithBinder { get; }
        }

        private class MyClass9
        {
            [Alias("property.name.1")]
            [Alias("42_property_name")]
            [Alias("$prop")]
            public bool PropertyWithAlias { get; }
        }

        private class MyClass10
        {
            [Alias("alias")]
            [Alias("propertywithalias")]
            public bool PropertyWithAlias { get; }
        }

        private struct MyStruct
        {
            public bool Property1 { get; set; }
            public bool Property2 { get; set; }
        }

        public class MyStringBinder : ISettingsBinder<string>
        {
            public string Bind(ISettingsNode rawSettings) => "My" + rawSettings.Value;
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