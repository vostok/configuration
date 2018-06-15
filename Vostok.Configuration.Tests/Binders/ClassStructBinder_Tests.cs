using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Tests.Binders
{
    public class StructBinder_Tests : Binders_Test
    {
        [Test]
        public void Should_bind_to_Struct_of_primitives()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    ["Int"] = new ValueNode("10", "Int"),
                    ["String"] = new ValueNode("str", "String"),
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
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    ["Int"] = new ValueNode("10"),
                    //["Long"] = new ValueNode("1234567890")},  is optional
                    //["String"] = new ValueNode("str")},       is optional
                    ["Float"] = new ValueNode("1,23"),
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedStructByProps {Int = 10, Float = 1.23f});
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_struct_attributes()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    ["Int"] = new ValueNode("10"),
                    ["Long"] = new ValueNode("123456789012"),
                    ["String"] = new ValueNode("str"),
                    ["Float"] = new ValueNode("not float number"),
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructGlobal>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedStructGlobal {Int = 10, Long = 123456789012L, String = "str", Float = 0});
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_wrong_value()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>()
                {
                    ["Float"] = new ValueNode("not float number"),
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_no_value_in_settings()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    ["wrong_key_name"] = new ValueNode("1.23"),
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_null_value()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    ["Float"] = new ValueNode(null),
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
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Int", new ValueNode("10")},
                    {"Strings", new ArrayNode(new List<ISettingsNode>
                    {
                        new ValueNode("str"),
                        new ValueNode("qwe"),
                    })},
                });
            var binder = Container.GetInstance<ISettingsBinder<SimpleClass>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new SimpleClass { Int = 10, Strings = new []{ "str", "qwe" } });
        }

        [Test]
        public void Should_bind_to_Class_with_class_field()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Int", new ValueNode("10")},
                    {"SimpleClass", new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        { "Int", new ValueNode("123") },
                    })},
                });
            var binder = Container.GetInstance<ISettingsBinder<ClassWithClass>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new ClassWithClass { Int = 10, SimpleClass = new SimpleClass { Int = 123 } });
        }

        [Test]
        public void Should_bind_to_Class_considering_prop_attributes()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Int", new ValueNode("10")},
                    //{"Long", new ValueNode("1234567890")},    is optional
                    //{"Strings", new ValueNode(...)},          is optional
                    {"Float", new ValueNode("1,23")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedClassByProps { Int = 10, Float = 1.23f });
        }

        [Test]
        public void Should_get_default_value_is_cannot_parse_value()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Int", new ValueNode("wrong value")},
                    {"Long", new ValueNode("wrong value")},
                    {"Bools", new ArrayNode(new List<ISettingsNode>
                    {
                        new ValueNode("wrong value"),
                    })},
                });
            var binder = Container.GetInstance<ISettingsBinder<ClassWithDefaults>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(new ClassWithDefaults());
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_struct_attributes()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Ints", new ArrayNode(new List<ISettingsNode>
                    {
                        new ValueNode("1"),
                        new ValueNode("2"),
                    })},
                    {"Long", new ValueNode("123456789012")},
                    {"String", new ValueNode("str")},
                    {"Float", new ValueNode("not float number")},
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
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Float", new ValueNode("not float number")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_no_value_in_settings()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"wrong_key_name", new ValueNode("1.23")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_null_value()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Float", new ValueNode(null)},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedClassByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_validate_with_ValidateBy_attribute()
        {
            var settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Str", new ValueNode("")},
                    {"Int", new ValueNode("-1")},
                });
            var binder = Container.GetInstance<ISettingsBinder<ValidatedClass>>();
            new Action(() => binder.Bind(settings)).Should().Throw<FormatException>();

            settings = new ObjectNode(
                new SortedDictionary<string, ISettingsNode>
                {
                    {"Str", new ValueNode("qwe")},
                    {"Int", new ValueNode("1")},
                });
            binder = Container.GetInstance<ISettingsBinder<ValidatedClass>>();
            binder.Bind(settings).Should().BeEquivalentTo(new ValidatedClass {Str = "qwe", Int = 1});

            var wrongBinder = Container.GetInstance<ISettingsBinder<WrongValidatedClass>>();
            new Action(() => wrongBinder.Bind(settings)).Should().Throw<FormatException>();
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

        private class MyValidator: IValidator
        {
            public IReadOnlyDictionary<string, string> Errors { get; private set; }

            public void Validate<T>(T obj)
            {
                if (IsValid(obj)) return;

                var sb = new StringBuilder($"{typeof(T).Name} validation exception:\r\n");
                foreach (var pair in Errors)
                    sb.AppendLine($"{pair.Key}: {pair.Value}");
                throw new FormatException(sb.ToString());
            }

            public bool IsValid<T>(T obj)
            {
                var errors = new Dictionary<string, string>();
                if (obj == null)
                {
                    errors["Null"] = "object is null";
                    Errors = errors;
                    return false;
                }

                if (!(obj is ValidatedClass objct))
                {
                    errors["Type"] = "wrong type";
                    Errors = errors;
                    return false;
                }

                if (string.IsNullOrEmpty(objct.Str))
                    errors[nameof(objct.Str)] = "empty or null";
                if (objct.Int < 0)
                    errors[nameof(objct.Int)] = "negative";

                if (errors.Any())
                {
                    Errors = errors;
                    return false;
                }
                return true;
            }
        }

        [ValidateBy(typeof(MyValidator))]
        private class ValidatedClass
        {
            public string Str { get; set; }
            public int Int;
        }

        [ValidateBy(typeof(MyValidator))]
        private class WrongValidatedClass
        {
            public string Str { get; set; }
            public int Int;
        }
    }
}