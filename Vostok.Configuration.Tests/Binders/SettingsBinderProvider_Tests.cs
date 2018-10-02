using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    public class SettingsBinderProvider_Tests
    {
        private SettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            provider = new SettingsBinderProvider();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_boolean()
        {
            provider.CreateFor<bool>().Should().BeOfType<PrimitiveAndSimpleBinder<bool>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_signed_integral_types()
        {
            provider.CreateFor<sbyte>().Should().BeOfType<PrimitiveAndSimpleBinder<sbyte>>();
            provider.CreateFor<short>().Should().BeOfType<PrimitiveAndSimpleBinder<short>>();
            provider.CreateFor<int>().Should().BeOfType<PrimitiveAndSimpleBinder<int>>();
            provider.CreateFor<long>().Should().BeOfType<PrimitiveAndSimpleBinder<long>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_unsigned_integral_types()
        {
            provider.CreateFor<byte>().Should().BeOfType<PrimitiveAndSimpleBinder<byte>>();
            provider.CreateFor<ushort>().Should().BeOfType<PrimitiveAndSimpleBinder<ushort>>();
            provider.CreateFor<uint>().Should().BeOfType<PrimitiveAndSimpleBinder<uint>>();
            provider.CreateFor<ulong>().Should().BeOfType<PrimitiveAndSimpleBinder<ulong>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_floating_point_types()
        {
            provider.CreateFor<float>().Should().BeOfType<PrimitiveAndSimpleBinder<float>>();
            provider.CreateFor<double>().Should().BeOfType<PrimitiveAndSimpleBinder<double>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_char()
        {
            provider.CreateFor<char>().Should().BeOfType<PrimitiveAndSimpleBinder<char>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_IntPtr()
        {
            provider.CreateFor<IntPtr>().Should().BeOfType<PrimitiveAndSimpleBinder<IntPtr>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_UIntPtr()
        {
            provider.CreateFor<UIntPtr>().Should().BeOfType<PrimitiveAndSimpleBinder<UIntPtr>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_string()
        {
            provider.CreateFor<string>().Should().BeOfType<PrimitiveAndSimpleBinder<string>>();
        }

        [Test]
        public void Should_select_PrimitiveAndSimpleBinder_for_custom_configured_type()
        {
            provider.WithParserFor<MyClass>(MyClass.TryParse);

            provider.CreateFor<MyClass>().Should().BeOfType<PrimitiveAndSimpleBinder<MyClass>>();
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
        public void Should_select_ClassAndStructBinder_for_unknown_classes()
        {
            provider.CreateFor<MyClass>().Should().BeOfType<ClassAndStructBinder<MyClass>>();
        }

        [Test]
        public void Should_select_ClassAndStructBinder_for_unknown_structs()
        {
            provider.CreateFor<MyStruct>().Should().BeOfType<ClassAndStructBinder<MyStruct>>();
        }

        [Test]
        public void Should_select_ClassAndStructBinder_for_unknown_generic_classes()
        {
            provider.CreateFor<MyClass2<int>>().Should().BeOfType<ClassAndStructBinder<MyClass2<int>>>();
        }

        [Test]
        public void Should_select_ClassAndStructBinder_for_unknown_generic_structs()
        {
            provider.CreateFor<MyStruct2<int>>().Should().BeOfType<ClassAndStructBinder<MyStruct2<int>>>();
        }

        public class MyClass
        {
            public static bool TryParse(string s, out MyClass v)
            {
                v = new MyClass();
                return true;
            }
        }

        public class MyClass2<T>
        {

        }

        public struct MyStruct
        {
            
        }

        public struct MyStruct2<T>
        {
            
        }
    }
}