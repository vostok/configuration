using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class StructBinder_Tests : Binders_Test
    {
        [Test]
        public void Should_bind_to_Struct_of_primitives()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    {"String", new SettingsNode("str", "String")},
                });
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct>>();
            var result = binder.Bind(settings);
            result.Should()
                .BeEquivalentTo(
                    new SimpleStruct {Int = 10, String = "str"});
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_prop_attributes()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    //{"Long", new RawSettings("1234567890", "Long")},  is optional
                    //{"String", new RawSettings("str", "String")},   is optional
                    {"Float", new SettingsNode("1,23", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            var result = binder.Bind(settings);
            result.Should()
                .BeEquivalentTo(
                    new AttributedStructByProps {Int = 10, Float = 1.23f});
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_struct_attributes()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    {"Long", new SettingsNode("123456789012", "Long")},
                    {"String", new SettingsNode("str", "String")},
                    {"Float", new SettingsNode("not float number", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructGlobal>>();
            var result = binder.Bind(settings);
            result.Should()
                .BeEquivalentTo(
                    new AttributedStructGlobal {Int = 10, Long = 123456789012L, String = "str", Float = 0});
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_wrong_value()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Float", new SettingsNode("not float number", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_no_value_in_settings()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"wrong_key_name", new SettingsNode("1.23", "wrong_key_name")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_null_value()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Float", new SettingsNode(null, "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        private struct SimpleStruct
        {
            public int Int { get; set; }
            public string String;
        }

        private struct AttributedStructByProps
        {
            [Optional]
            public int Int { get; set; }
            [Optional]
            public long? Long;
            [Optional]
            public string String { get; set; }
            [Required]
            public float? Float;
        }

        [RequiredByDefault]
        private struct AttributedStructGlobal
        {
            public int Int;
            public long? Long;
            public string String { get; set; }
            [Optional]
            public float Float;
        }
    }

    public class ClassBinder_Tests : Binders_Test
    {
        [Test]
        public void Should_bind_to_Class()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    {"Strings", new SettingsNode(new OrderedDictionary
                    {
                        ["1"] = new SettingsNode("str", "1"),
                        ["2"] = new SettingsNode("qwe", "2"),
                    }, "Strings")},
                });
            var binder = Container.GetInstance<ISettingsBinder<SimpleClass>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new SimpleClass { Int = 10, Strings = new []{ "str", "qwe" } });
        }

        [Test]
        public void Should_bind_to_Class_with_class_field()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    {"SimpleClass", new SettingsNode(new OrderedDictionary
                    {
                        { "Int", new SettingsNode("123", "Int") },
                    }, "SimpleClass")},
                });
            var binder = Container.GetInstance<ISettingsBinder<ClassWithClass>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new ClassWithClass { Int = 10, SimpleClass = new SimpleClass { Int = 123 } });
        }

        [Test]
        public void Should_bind_to_Class_considering_prop_attributes()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("10", "Int")},
                    //{"Long", new RawSettings("1234567890")},  is optional
                    //{"Strings", new RawSettings(...)},    is optional
                    {"Float", new SettingsNode("1,23", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedClassByProps { Int = 10, Float = 1.23f });
        }

        [Test]
        public void Should_get_default_value_is_cannot_parse_value()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Int", new SettingsNode("wrong value", "Int")},
                    {"Long", new SettingsNode("wrong value")},
                    {"Bools", new SettingsNode(new OrderedDictionary
                    {
                        ["0"] = new SettingsNode("wrong value"),
                    })},
                });
            var binder = Container.GetInstance<ISettingsBinder<ClassWithDefaults>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new ClassWithDefaults());
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_struct_attributes()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Ints", new SettingsNode(new OrderedDictionary
                    {
                        ["1"] = new SettingsNode("1"),
                        ["2"] = new SettingsNode("2"),
                    }, "Ints")},
                    {"Long", new SettingsNode("123456789012", "Long")},
                    {"String", new SettingsNode("str", "String")},
                    {"Float", new SettingsNode("not float number", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClasGlobal>>();
            var result = binder.Bind(settings);
            result.Long.Should().Be(123456789012L);
            result.String.Should().Be("str");
            result.Float.Should().Be(0);
            result.Ints.Should().Equal(new List<int> {1, 2});
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_wrong_value()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Float", new SettingsNode("not float number", "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_no_value_in_settings()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"wrong_key_name", new SettingsNode("1.23", "wrong_key_name")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_null_value()
        {
            var settings = new SettingsNode(
                new OrderedDictionary
                {
                    {"Float", new SettingsNode(null, "Float")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        private class SimpleClass
        {
            public int Int { get; set; }
            public string[] Strings;
        }

        private class ClassWithClass
        {
            public int Int { get; set; }
            public SimpleClass SimpleClass;
        }

        private class AttributedClassByProps
        {
            [Optional]
            public int Int { get; set; }
            [Optional]
            public long? Long;
            [Optional]
            public List<string> Strings { get; set; }
            [Required]
            public float? Float;
        }

        [RequiredByDefault]
        private struct AttributedClasGlobal
        {
            public List<int> Ints;
            public long? Long;
            public string String { get; set; }
            [Optional]
            public float Float;
        }

        private class ClassWithDefaults
        {
            public int Int { get; set; } = 123;
            public long? Long = 321;
            public List<bool> Bools { get; set; } = new List<bool> { true };
        }
    }
}