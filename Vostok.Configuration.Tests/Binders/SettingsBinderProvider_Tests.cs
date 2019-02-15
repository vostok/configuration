using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Collection;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    public class SettingsBinderProvider_Tests
    {
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            provider = new SettingsBinderProvider().WithDefaultParsers();
        }

        [Test]
        public void Should_select_CustomBinderWrapper_when_configured_for_unknown_classes()
        {
            provider.SetupCustomBinder(new MyClassBinder());
            ShouldBeCustomBinderWrapperOver<MyClassBinder, MyClass>(provider.CreateFor<MyClass>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_when_configured_for_unknown_generic_classes()
        {
            provider.SetupCustomBinder(new MyClass2Binder<int>());
            ShouldBeCustomBinderWrapperOver<MyClass2Binder<int>, MyClass2<int>>(provider.CreateFor<MyClass2<int>>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_when_configured_for_unknown_structs()
        {
            provider.SetupCustomBinder(new MyStructBinder());
            ShouldBeCustomBinderWrapperOver<MyStructBinder, MyStruct>(provider.CreateFor<MyStruct>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_when_configured_for_unknown_generic_structs()
        {
            provider.SetupCustomBinder(new MyStruct2Binder<int>());
            ShouldBeCustomBinderWrapperOver<MyStruct2Binder<int>, MyStruct2<int>>(provider.CreateFor<MyStruct2<int>>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_boolean()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<bool>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_signed_integral_types()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<sbyte>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<short>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<int>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<long>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_unsigned_integral_types()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<byte>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<ushort>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<uint>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<ulong>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_floating_point_types()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<float>());
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<double>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_char()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<char>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_string()
        {
            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<string>());
        }

        [Test]
        public void Should_select_CustomBinderWrapper_over_PrimitiveBinder_for_custom_configured_type()
        {
            provider.WithParserFor<MyClass>(MyClass.TryParse);

            ShouldBeCustomBinderWrapperOverPrimitiveBinder(provider.CreateFor<MyClass>());
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
        public void Should_select_ClassStructBinder_for_unknown_classes()
        {
            provider.CreateFor<MyClass>().Should().BeOfType<ClassStructBinder<MyClass>>();
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
            new Action(() => provider.SetupCustomBinder(Substitute.For<ISettingsBinder<MyClass>>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_throw_when_SetupParserFor_called_for_type_after_CreateFor()
        {
            provider.CreateFor<MyClass>();
            new Action(() => provider.SetupParserFor<MyClass>(Substitute.For<ITypeParser>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        private void ShouldBeCustomBinderWrapperOver<TBinder, TSettings>(ISettingsBinder<TSettings> binder)
        {
            binder.Should().BeOfType<CustomBinderWrapper<TSettings>>();
            ((CustomBinderWrapper<TSettings>)binder).Binder.Should().BeOfType<TBinder>();
        }

        private void ShouldBeCustomBinderWrapperOverPrimitiveBinder<T>(ISettingsBinder<T> binder)
        {
            ShouldBeCustomBinderWrapperOver<PrimitiveBinder<T>, T>(binder);
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
        
        public class MyClassBinder: ISettingsBinder<MyClass>
        {
            public MyClass Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyClass2Binder<T>: ISettingsBinder<MyClass2<T>>
        {
            public MyClass2<T> Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyStructBinder: ISettingsBinder<MyStruct>
        {
            public MyStruct Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
        
        public class MyStruct2Binder<T>: ISettingsBinder<MyStruct2<T>>
        {
            public MyStruct2<T> Bind(ISettingsNode rawSettings) =>
                throw new NotImplementedException();
        }
    }
}