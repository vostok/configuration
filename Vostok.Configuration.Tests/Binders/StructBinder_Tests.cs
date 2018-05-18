using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class StructBinder_Tests: Binders_Test
    {
        [Test]
        public void Should_bind_to_Struct_of_primitives()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"Int", new RawSettings("10")},
                    {"String", new RawSettings("str")},
                });
            var binder = Container.GetInstance<ISettingsBinder<SimpleStruct>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new SimpleStruct {Int = 10, String = "str"});
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_prop_attributes()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"Int", new RawSettings("10")},
                    //{"Long", new RawSettings("1234567890")},  is optional
                    //{"String", new RawSettings("str")},   is optional
                    {"Float", new RawSettings("1,23")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedStructByProps {Int = 10, Float = 1.23f});
        }

        [Test]
        public void Should_bind_to_Struct_of_primitives_considering_struct_attributes()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"Int", new RawSettings("10")},
                    {"Long", new RawSettings("123456789012")},
                    {"String", new RawSettings("str")},
                    {"Float", new RawSettings("not float number")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructGlobal>>();
            var result = binder.Bind(settings);
            result.Should().BeEquivalentTo(
                new AttributedStructGlobal { Int = 10, Long = 123456789012L, String = "str", Float = 0 });
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_wrong_value()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"Float", new RawSettings("not float number")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_no_value_in_settings()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"wrong_key_name", new RawSettings("1.23")},
                });
            var binder = Container.GetInstance<ISettingsBinder<AttributedStructByProps>>();
            new Action(() => binder.Bind(settings)).Should().Throw<Exception>();
        }

        [Test]
        public void Should_throw_exception_if_requred_value_has_null_value()
        {
            var settings = new RawSettings(
                new Dictionary<string, RawSettings>
                {
                    {"Float", new RawSettings(null)},
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
}