using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

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
                .Should().Throw<Exception>().Which.ShouldBePrinted();
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
        public void Should_leave_settings_as_is_when_binding_to_ISettingsNode()
        {
            var tree = Object("xx", Array("yy", Value("zz")));

            var result = binder.Bind<ISettingsNode>(tree);
            result.Should().Be(tree);
        }

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
            
            private class CustomConfigBinder : ISafeSettingsBinder<CustomBinderConfig>
            {
                public SettingsBindingResult<CustomBinderConfig> Bind(ISettingsNode rawSettings) => 
                    SettingsBindingResult.Success(new CustomBinderConfig { Value = rawSettings.Value?.ToUpper() });
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
        
        private class MyListBinder<T> : ISafeSettingsBinder<MyList<T>>
        {
            private readonly ISafeSettingsBinder<T> innerBinder;

            public MyListBinder(ISafeSettingsBinder<T> innerBinder) => this.innerBinder = innerBinder;

            public SettingsBindingResult<MyList<T>> Bind(ISettingsNode rawSettings)
            {
                return SettingsBindingResult.Success(
                    new MyList<T>(rawSettings.Children.Select(c => innerBinder.Bind(c).Value)));
            }
        }
    }
}