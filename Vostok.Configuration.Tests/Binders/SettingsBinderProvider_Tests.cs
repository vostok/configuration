using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Binders.Extensions;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    internal class SettingsBinderProvider_Tests
    {
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            provider = new SettingsBinderProvider().WithDefaultParsers();
        }

        [Test]
        public void Should_select_custom_binder_when_configured_for_unknown_classes()
        {
            provider.SetupCustomBinder(new MyClassBinder());
            provider.CreateFor<MyClass>().Should().BeOfType<MyClassBinder>();
        }

        [Test]
        public void Should_select_custom_binder_when_configured_for_unknown_generic_classes()
        {
            provider.SetupCustomBinder(new MyClass2Binder<int>());
            provider.CreateFor<MyClass2<int>>().Should().BeOfType<MyClass2Binder<int>>();
        }

        [Test]
        public void Should_select_custom_binder_when_configured_for_unknown_structs()
        {
            provider.SetupCustomBinder(new MyStructBinder());
            provider.CreateFor<MyStruct>().Should().BeOfType<MyStructBinder>();
        }

        [Test]
        public void Should_select_custom_binder_when_configured_for_unknown_generic_structs()
        {
            provider.SetupCustomBinder(new MyStruct2Binder<int>());
            provider.CreateFor<MyStruct2<int>>().Should().BeOfType<MyStruct2Binder<int>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_boolean()
        {
            provider.CreateFor<bool>().Should().BeOfType<PrimitiveBinder<bool>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_signed_integral_types()
        {
            provider.CreateFor<sbyte>().Should().BeOfType<PrimitiveBinder<sbyte>>();
            provider.CreateFor<short>().Should().BeOfType<PrimitiveBinder<short>>();
            provider.CreateFor<int>().Should().BeOfType<PrimitiveBinder<int>>();
            provider.CreateFor<long>().Should().BeOfType<PrimitiveBinder<long>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_unsigned_integral_types()
        {
            provider.CreateFor<byte>().Should().BeOfType<PrimitiveBinder<byte>>();
            provider.CreateFor<ushort>().Should().BeOfType<PrimitiveBinder<ushort>>();
            provider.CreateFor<uint>().Should().BeOfType<PrimitiveBinder<uint>>();
            provider.CreateFor<ulong>().Should().BeOfType<PrimitiveBinder<ulong>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_floating_point_types()
        {
            provider.CreateFor<float>().Should().BeOfType<PrimitiveBinder<float>>();
            provider.CreateFor<double>().Should().BeOfType<PrimitiveBinder<double>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_char()
        {
            provider.CreateFor<char>().Should().BeOfType<PrimitiveBinder<char>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_string()
        {
            provider.CreateFor<string>().Should().BeOfType<PrimitiveBinder<string>>();
        }

        [Test]
        public void Should_select_PrimitiveBinder_for_custom_configured_type()
        {
            provider.WithParserFor<MyClass>(MyClass.TryParse);

            provider.CreateFor<MyClass>().Should().BeOfType<PrimitiveBinder<MyClass>>();
        }

        [Test]
        public void Should_select_NullableBinder_for_nullable_primitive_types()
        {
            provider.CreateFor<int?>().Should().BeOfType<NullableBinder<int>>();
            provider.CreateFor<bool?>().Should().BeOfType<NullableBinder<bool>>();
            provider.CreateFor<double?>().Should().BeOfType<NullableBinder<double>>();
        }

        [Test]
        public void Should_select_NullableBinder_for_nullable_structs()
        {
            provider.CreateFor<MyStruct?>().Should().BeOfType<NullableBinder<MyStruct>>();
        }

        [Test]
        public void Should_select_EnumBinder_for_enums()
        {
            provider.CreateFor<ConsoleColor>().Should().BeOfType<EnumBinder<ConsoleColor>>();
        }

        [Test]
        public void Should_select_ReadOnlyListBinder_for_arrays_of_primitive_type()
        {
            provider.CreateFor<int[]>().Should().BeOfType<ReadOnlyListBinder<int>>();
            provider.CreateFor<bool[]>().Should().BeOfType<ReadOnlyListBinder<bool>>();
        }

        [Test]
        public void Should_select_ReadOnlyListBinder_for_arrays_of_custom_type()
        {
            provider.CreateFor<MyClass[]>().Should().BeOfType<ReadOnlyListBinder<MyClass>>();
            provider.CreateFor<MyStruct[]>().Should().BeOfType<ReadOnlyListBinder<MyStruct>>();
        }

        [Test]
        public void Should_select_ListBinder_for_List()
        {
            provider.CreateFor<List<string>>().Should().BeOfType<ListBinder<string>>();
        }

        [Test]
        public void Should_select_ListBinder_for_IList()
        {
            provider.CreateFor<IList<string>>().Should().BeOfType<ListBinder<string>>();
        }

        [Test]
        public void Should_select_ListBinder_for_ICollection()
        {
            provider.CreateFor<ICollection<string>>().Should().BeOfType<ListBinder<string>>();
        }

        [Test]
        public void Should_select_ReadOnlyListBinder_for_IEnumerable()
        {
            provider.CreateFor<IEnumerable<string>>().Should().BeOfType<ReadOnlyListBinder<string>>();
        }

        [Test]
        public void Should_select_ReadOnlyListBinder_for_IReadOnlyList()
        {
            provider.CreateFor<IReadOnlyList<string>>().Should().BeOfType<ReadOnlyListBinder<string>>();
        }

        [Test]
        public void Should_select_ReadOnlyListBinder_for_IReadOnlyCollection()
        {
            provider.CreateFor<IReadOnlyCollection<string>>().Should().BeOfType<ReadOnlyListBinder<string>>();
        }

        [Test]
        public void Should_select_SetBinder_for_HashSet()
        {
            provider.CreateFor<HashSet<string>>().Should().BeOfType<SetBinder<string>>();
        }

        [Test]
        public void Should_select_SetBinder_for_ISet()
        {
            provider.CreateFor<ISet<string>>().Should().BeOfType<SetBinder<string>>();
        }

        [Test]
        public void Should_select_DictionaryBinder_for_Dictionary()
        {
            provider.CreateFor<Dictionary<string, int>>().Should().BeOfType<DictionaryBinder<string, int>>();
        }

        [Test]
        public void Should_select_DictionaryBinder_for_IDictionary()
        {
            provider.CreateFor<IDictionary<string, int>>().Should().BeOfType<DictionaryBinder<string, int>>();
        }

        [Test]
        public void Should_select_DictionaryBinder_for_IReadOnlyDictionary()
        {
            provider.CreateFor<IReadOnlyDictionary<string, int>>().Should().BeOfType<DictionaryBinder<string, int>>();
        }

        [Test]
        public void Should_select_ParseMethodBinder_for_classes_with_parse_method()
        {
            provider.CreateFor<MyClass>().Should().BeOfType<ParseMethodBinder<MyClass>>();
        }

        [Test]
        public void Should_select_ClassStructBinder_for_unknown_classes()
        {
            provider.CreateFor<UnknownClass>().Should().BeOfType<ClassStructBinder<UnknownClass>>();
        }

        [Test]
        public void Should_select_ClassStructBinder_for_unknown_structs()
        {
            provider.CreateFor<MyStruct>().Should().BeOfType<ClassStructBinder<MyStruct>>();
        }

        [Test]
        public void Should_select_ClassStructBinder_for_unknown_generic_classes()
        {
            provider.CreateFor<MyClass2<int>>().Should().BeOfType<ClassStructBinder<MyClass2<int>>>();
        }

        [Test]
        public void Should_select_ClassStructBinder_for_unknown_generic_structs()
        {
            provider.CreateFor<MyStruct2<int>>().Should().BeOfType<ClassStructBinder<MyStruct2<int>>>();
        }

        [Test]
        public void Should_throw_when_SetupCustomBinder_called_for_type_after_CreateFor()
        {
            provider.CreateFor<MyClass>();
            new Action(() => provider.SetupCustomBinder(Substitute.For<ISafeSettingsBinder<MyClass>>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_select_binder_specified_in_BindBy_attribute()
        {
            var wrapper = provider.CreateFor<ClassWithCustomBinder>();
            ((SafeBinderWrapper<ClassWithCustomBinder>)wrapper).Binder.Should().BeOfType<CustomBinder>();
        }

        [Test]
        public void Should_throw_when_SetupParserFor_called_for_type_after_CreateFor()
        {
            provider.CreateFor<MyClass>();
            new Action(() => provider.SetupParserFor<MyClass>(Substitute.For<ITypeParser>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_select_ConstructorBinder()
        {
            provider.CreateFor<MyClass3>().Should().BeOfType<ConstructorBinder<MyClass3>>();
        }

        public class MyClass
        {
            public static bool TryParse(string s, out MyClass v)
            {
                v = new MyClass();
                return true;
            }
        }

        public class UnknownClass
        {
        }

        public class MyClass2<T>
        {

        }

        public class MyClass3
        {
            public MyClass3(string value)
            {
            }
        }

        public struct MyStruct
        {
            
        }

        public struct MyStruct2<T>
        {
            
        }
        
        public class MyClassBinder: ISafeSettingsBinder<MyClass>
        {
            public SettingsBindingResult<MyClass> Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyClass2Binder<T>: ISafeSettingsBinder<MyClass2<T>>
        {
            public SettingsBindingResult<MyClass2<T>> Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyStructBinder: ISafeSettingsBinder<MyStruct>
        {
            public SettingsBindingResult<MyStruct> Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyStruct2Binder<T>: ISafeSettingsBinder<MyStruct2<T>>
        {
            public SettingsBindingResult<MyStruct2<T>>Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }

        [BindBy(typeof(CustomBinder))]
        public class ClassWithCustomBinder
        {
            
        }
        
        public class CustomBinder : ISettingsBinder<ClassWithCustomBinder>
        {
            public ClassWithCustomBinder Bind(ISettingsNode rawSettings) => throw new NotImplementedException();
        }
    }
}