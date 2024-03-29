﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Integration
{
    public class DefaultSettingsBinder_Tests : TreeConstructionSet
    {
        private DefaultSettingsBinder binder;

        [SetUp]
        public void TestSetup()
        {
            binder = new DefaultSettingsBinder();
        }

        [TestCase("42", 42)]
        [TestCase("text", "text")]
        [TestCase("Blue", ConsoleColor.Blue)]
        public void Should_bind_single_value_node_to_naked_type<T>(string rawValue, T expectedValue)
        {
            var tree = Value(rawValue);

            binder.Bind<T>(tree).Should().Be(expectedValue);
        }

        [TestCase("42", 42)]
        public void Should_bind_single_value_node_to_naked_type(string rawValue, int? expectedValue)
        {
            var tree = Value(rawValue);

            binder.Bind<int?>(tree).Should().Be(expectedValue);
        }

        [Test]
        public void Should_bind_base64_to_byte_array()
        {
            var tree = Value("CgsM/w==");

            binder.Bind<byte[]>(tree).Should().BeEquivalentTo(new byte[] {10, 11, 12, 255}, o => o.WithStrictOrdering());
        }

        // TODO(krait): Setup cases for other tests for naked type handling.

        [Test]
        public void Should_bind_named_single_value_node_to_naked_type()
        {
            var tree = Value("Name", "42");

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_array_node_with_single_child_to_naked_type()
        {
            var tree = Array(Value("42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_named_array_node_with_single_child_to_naked_type()
        {
            var tree = Array("Name", Value("42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_object_node_with_single_child_to_naked_type()
        {
            var tree = Object(("Key", "42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_named_object_node_with_single_child_to_naked_type()
        {
            var tree = Object("Name", ("Key", "42"));

            binder.Bind<int>(tree).Should().Be(42);
        }

        [Test]
        public void Should_bind_complex_stuff()
        {
            var tree = Array(
                Object(
                    Object("innerObject", Array("anotherArray", "167")),
                    Array("innerArray", "Yellow", "Red", "Black"),
                    Value("innerRegex", @"\d+"),
                    Value("nullableInt", "null"),
                    Value("customBinderObject", "xx")
                ),
                Value("null")
            );

            var result = binder.WithParserFor<Regex>(TryParseRegex).Bind<IEnumerable<ComplexConfig>>(tree);

            result.Should().HaveCount(2);
            result.Last().Should().BeNull();

            result.First().StringWithDefault.Should().Be("default");
            result.First().InnerRegex.ToString().Should().Be(@"\d+");
            result.First().InnerArray.Should().Equal(ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Black);
            result.First().InnerObject.AnotherArray.Should().Equal(167);
            result.First().NullableInt.Should().BeNull();
            result.First().CustomBinderObject.Value.Should().Be("XX");
        }

        [Test]
        public void Should_print_out_errors_nicely()
        {
            var tree = Object(
                Array("ListOfLists", Array(Value("1"), Value("2"), Value("xx"))),
                Array("SetOfInts", "10", "xx", "20"),
                Array("ListOfObjects",
                    Object(
                        Object("innerObject", Value("anotherArray", "xxx"))
                    )),
                Object("JustAProperty", Value("zz", "zz")),
                Array("NestedDictionaries",
                    Array("zz40", Value("key1", "300")),
                    Array("50", Value("key2", "100"), Value("key3", "yy"))
                )
            );

            new Action(() => binder.Bind<ComplexConfig2>(tree))
                .Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_not_allow_to_set_required_property_to_null()
        {
            var tree = Object(Value("RequiredProperty", null));

            new Action(() => binder.Bind<MyClass>(tree)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_not_allow_to_set_required_property_to_node_of_invaild_type()
        {
            var tree = Object(Object("RequiredProperty", new ISettingsNode[0]));

            new Action(() => binder.Bind<MyClass>(tree)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_not_allow_to_set_required_property_to_nested_null()
        {
            var tree = Object(Object("RequiredProperty", ("Value", null)));

            new Action(() => binder.Bind<MyClass>(tree)).Should().Throw<SettingsBindingException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_support_custom_collections()
        {
            binder.WithCustomBinder(typeof(MyListBinder<>), _ => true);

            var tree = Array(Array(Value("1"), Value("2")), Array(Value("3"), Value("4")));

            var result = binder.Bind<MyList<List<int>>>(tree);

            result.Should()
                .BeEquivalentTo(
                    new MyList<List<int>>(
                        new[]
                        {
                            new List<int> {1, 2},
                            new List<int> {3, 4},
                        }));
        }

        [Test]
        public void Should_support_custom_collections_inside_custom_types()
        {
            binder.WithCustomBinder(typeof(MyListBinder<>), _ => true);

            var tree = Object(Array("MyList", "1", "2", "3"));

            var result = binder.Bind<MyConfig>(tree);

            result.MyList.Should()
                .BeEquivalentTo(
                    new MyList<int>(new[] {1, 2, 3}));
        }

        [Test]
        public void Should_support_custom_collections_inside_custom_types_when_two_custom_binders()
        {
            binder.WithCustomBinder(typeof(MyListBinder<>), c => c == typeof(int));
            binder.WithCustomBinder(typeof(MyListBinderCopy<>), c => c != typeof(int));

            var tree = Object(Array("MyList", "1", "2", "3"));

            var result = binder.Bind<MyConfig>(tree);

            result.MyList.Should()
               .BeEquivalentTo(
                    new MyList<int>(new[] {1, 2, 3}));
        }

        [Test]
        public void Should_leave_settings_as_is_when_binding_to_ISettingsNode()
        {
            var tree = Object("xx", Array("yy", Value("zz")));

            var result = binder.Bind<ISettingsNode>(tree);
            result.Should().Be(tree);
        }

        [Test]
        public void Should_skip_fields_of_abstract_type_if_there_is_no_corresponding_binder()
        {
            var settings = Object(Object("AbstractField", ("xx", "yy")));

            binder.Bind<MyClass9>(settings).AbstractField.Should().BeNull();
        }

        [Test]
        public void Should_skip_properties_of_abstract_type_if_there_is_no_corresponding_binder()
        {
            var settings = Object(Object("AbstractProperty", ("xx", "yy")));

            binder.Bind<MyClass9>(settings).AbstractProperty.Should().BeNull();
        }

        [Test]
        public void Should_bind_fields_of_interface_type()
        {
            var settings = Object(Object("InterfaceField", ("xx", "yy")));

            binder.Bind<MyClass9>(settings).InterfaceField.Should().NotBeNull();
        }

        [Test]
        public void Should_bind_properties_of_interface_type()
        {
            var settings = Object(Object("InterfaceProperty", ("xx", "yy")));

            binder.Bind<MyClass9>(settings).InterfaceProperty.Should().NotBeNull();
        }

        [Test]
        public void Should_not_lose_instantiated_defaults_in_nested_classes()
        {
            var settings = Object(
                Object("nested1", ("A", "3")),
                Object("nested2", ("A", "4")),
                Array("nested3", Object(("A", "5"))));

            var model = binder.Bind<OuterConfig>(settings);

            model.Nested1.A.Should().Be(3);
            model.Nested1.B.Should().Be(2);

            model.Nested2.A.Should().Be(4);
            model.Nested2.B.Should().Be(0);

            var arrayElement = model.Nested3.Should().ContainSingle().Which;

            arrayElement.A.Should().Be(5);
            arrayElement.B.Should().Be(0);
        }

        [Test]
        public void Should_not_bind_object_without_binders_and_parameterless_constructors()
        {
            var settings = Object(("FirstValue", "5"));

            Action binding = () => binder.Bind<ConfigWithoutConstructor>(settings);

            binding.Should().Throw<Exception>();
        }

        [Test]
        public void Should_bind_object_uninitialized_without_binders_and_parameterless_constructors_with_OmitConstructors()
        {
            var settings = Object(("FirstValue", "5"));

            Func<ConfigOmitConstructors> binding = () => binder.Bind<ConfigOmitConstructors>(settings);

            binding.Should().NotThrow();

            var result = binding();

            result.FirstValue.Should().Be(5);
            result.SecondValue.Should().Be(0);
        }
        
        [Test]
        public void Should_bind_object_uninitialized_without_binders_with_OmitConstructors_when_parameterless_constructor_exists()
        {
            var settings = Object(("FirstValue", "5"));

            Func<ConfigOmitConstructorsWithConstructor> binding = () => binder.Bind<ConfigOmitConstructorsWithConstructor>(settings);

            binding.Should().NotThrow();

            var result = binding();

            result.FirstValue.Should().Be(5);
            result.SecondValue.Should().Be(0);
        }

        [Test]
        public void Should_bind_object_with_custom_binder_with_OmitConstructors_when_parameterless_constructor_exists()
        {
            var settings = Object(Object("ConfigOmitConstructorsWithConstructor", Value("FirstValue", "5")));

            Func<ComplexConfigWithParameterlessConfigInside> binding = () => binder.Bind<ComplexConfigWithParameterlessConfigInside>(settings);

            binding.Should().NotThrow();

            var result = binding();

            result.ConfigOmitConstructorsWithConstructor.FirstValue.Should().Be(5);
            result.ConfigOmitConstructorsWithConstructor.SecondValue.Should().Be(28);
        }

        [Test]
        public void Should_bind_non_generic()
        {
            var settings1 = Object(("Type", "1"), ("A", "5"));
            var settings2 = Object(("Type", "2"), ("A", "hello"));

            var result1 = binder.Bind(typeof(SettingsVariant1), settings1);
            var result2 = binder.Bind(typeof(SettingsVariant2), settings2);

            result1.Should().BeOfType<SettingsVariant1>().Which.A.Should().Be(5);
            result2.Should().BeOfType<SettingsVariant2>().Which.A.Should().Be("hello");
        }

        [Test]
        public void Should_bind_with_type_switching()
        {
            var settings1 = Object(("Type", "1"), ("A", "5"));
            var settings2 = Object(("Type", "2"), ("A", "hello"));
            
            var result1 = binder.Bind<BaseSettings>(settings1);
            var result2 = binder.Bind<BaseSettings>(settings2);

            result1.Should().BeOfType<SettingsVariant1>().Which.A.Should().Be(5);
            result2.Should().BeOfType<SettingsVariant2>().Which.A.Should().Be("hello");
        }

        [Test]
        public void Should_bind_with_type_switching_non_generic()
        {
            var settings1 = Object(("Type", "1"), ("A", "5"));
            var settings2 = Object(("Type", "2"), ("A", "hello"));

            var result1 = binder.Bind(typeof(BaseSettings), settings1);
            var result2 = binder.Bind(typeof(BaseSettings), settings2);

            result1.Should().BeOfType<SettingsVariant1>().Which.A.Should().Be(5);
            result2.Should().BeOfType<SettingsVariant2>().Which.A.Should().Be("hello");
        }

        private class MyClass9
        {
            public Abstract AbstractField;
            public Abstract AbstractProperty { get; set; }
            public IInterface InterfaceField;
            public IInterface InterfaceProperty { get; set; }
        }

        private abstract class Abstract {}
        public interface IInterface {}

        private static bool TryParseRegex(string s, out Regex regex)
        {
            regex = new Regex(s);
            return true;
        }

        private class ComplexConfig
        {
            public Regex InnerRegex;

            public List<ConsoleColor> InnerArray;

            public string StringWithDefault = "default";

            public InnerConfig InnerObject;

            public int? NullableInt;

            public CustomBinderConfig CustomBinderObject;


            public class InnerConfig
            {
                public int[] AnotherArray { get; set; }
            }

            [BindBy(typeof(CustomConfigBinder))]
            public class CustomBinderConfig
            {
                public string Value;
            }

            private class CustomConfigBinder : ISettingsBinder<CustomBinderConfig>
            {
                public CustomBinderConfig Bind(ISettingsNode rawSettings) =>
                    new CustomBinderConfig { Value = rawSettings?.Value?.ToUpper() };
            }
        }

        private class ComplexConfig2
        {
            public List<List<int>> NestedLists { get; }

            public List<ComplexConfig> ListOfObjects;

            public Dictionary<int, Dictionary<string, int>> NestedDictionaries { get; set; }

            public int JustAProperty { get; private set; }

            public HashSet<int> SetOfInts;
        }

        private class MyClass
        {
            [Required]
            public string RequiredProperty { get; set; }
        }

        private class MyList<T> : IEnumerable<T>
        {
            private readonly List<T> data;

            public MyList(IEnumerable<T> data) => this.data = data.ToList();

            public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)data).GetEnumerator();
        }

        private class MyListBinder<T> : ISettingsBinder<MyList<T>>
        {
            private readonly ISettingsBinder<T> innerBinder;

            public MyListBinder(ISettingsBinder<T> innerBinder) => this.innerBinder = innerBinder;

            public MyList<T> Bind(ISettingsNode rawSettings)
            {
                return new MyList<T>(rawSettings.Children.Select(c => innerBinder.Bind(c)));
            }
        }
        
        private class MyListBinderCopy<T> : MyListBinder<T>
        {
            public MyListBinderCopy(ISettingsBinder<T> innerBinder)
                : base(innerBinder)
            {
            }
        }

        private class MyConfig
        {
            public MyList<int> MyList;
        }

        private class OuterConfig
        {
            public NestedConfig Nested1 { get; } = new NestedConfig
            {
                A = 1,
                B = 2
            };

            public NestedConfig Nested2 { get; }

            public NestedConfig[] Nested3 = {};
        }

        private class NestedConfig
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        private class ConfigWithoutConstructor
        {
            public int FirstValue { get; set; }
            public int SecondValue { get; set; } = 10;

            public ConfigWithoutConstructor(int firstFirstValue, int secondValue = 15)
            {
                FirstValue = firstFirstValue;
                SecondValue = secondValue;
            }
        }
        
        [OmitConstructors]
        private class ConfigOmitConstructors
        {
            public int FirstValue { get; set; }
            public int SecondValue { get; set; } = 10;

            public ConfigOmitConstructors(int firstFirstValue, int secondValue = 15)
            {
                FirstValue = firstFirstValue;
                SecondValue = secondValue;
            }
        }

        [OmitConstructors]
        private class ConfigOmitConstructorsWithConstructor
        {
            public int FirstValue { get; set; }
            public int SecondValue { get; set; } = 10;
            
            public ConfigOmitConstructorsWithConstructor(){}

            public ConfigOmitConstructorsWithConstructor(int firstFirstValue, int secondValue = 15)
            {
                FirstValue = firstFirstValue;
                SecondValue = secondValue;
            }
        }

        private class ComplexConfigWithParameterlessConfigInside
        {
            [BindBy(typeof(BinderForConfigOmitConstructors))]
            public ConfigOmitConstructorsWithConstructor ConfigOmitConstructorsWithConstructor;
        }
        
        private class BinderForConfigOmitConstructors : ISettingsBinder<ConfigOmitConstructorsWithConstructor>
        {
            public ConfigOmitConstructorsWithConstructor Bind(ISettingsNode rawSettings)
            {
                return new ConfigOmitConstructorsWithConstructor {FirstValue = int.Parse(rawSettings["FirstValue"].Value), SecondValue = 28};
            }
        }
        
        [BindBy(typeof(BaseSettingsBinder))]
        private class BaseSettings 
        {
            public string Type {get; set;}
        }

        private class BaseSettingsBinder : ISettingsBinder<BaseSettings>
        {
            public static DefaultSettingsBinder DefaultSettingsBinder = new DefaultSettingsBinder();
            
            public BaseSettings Bind(ISettingsNode rawSettings)
            {
                switch (rawSettings.Children.FirstOrDefault(c => c.Name == nameof(BaseSettings.Type))?.Value)
                {
                    case "1": return DefaultSettingsBinder.Bind<SettingsVariant1>(rawSettings);
                    case "2": return DefaultSettingsBinder.Bind<SettingsVariant2>(rawSettings);
                    default: throw new ArgumentException();
                }
            }
        }

        private class SettingsVariant1 : BaseSettings
        {
            public int A { get; set; }
        }

        private class SettingsVariant2 : BaseSettings
        {
            public string A { get; set; }
        }
    }
}