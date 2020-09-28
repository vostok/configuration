using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vostok.Configuration.Binders.Extensions;

namespace Vostok.Configuration.Helpers
{
    internal static class DynamicTypesHelper
    {
        private const TypeAttributes DefaultTypeAttributes =
            TypeAttributes.Public |
            TypeAttributes.Class |
            TypeAttributes.AutoClass |
            TypeAttributes.AnsiClass |
            TypeAttributes.AutoLayout;

        private const MethodAttributes DefaultMethodAttributes =
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.Virtual |
            MethodAttributes.SpecialName;

        private const string CurrentInstanceFieldName = "<instance>";

        private const string Default = "default";
        private const string DynamicAssemblyName = "Vostok.Configuration.Dynamic";

        private static readonly ConcurrentDictionary<Type, Lazy<Type>> typesCache = new ConcurrentDictionary<Type, Lazy<Type>>();
        private static readonly ConcurrentDictionary<Type, Lazy<Type>> wrappersCache = new ConcurrentDictionary<Type, Lazy<Type>>();
        private static readonly ConcurrentDictionary<string, Lazy<ModuleBuilder>> moduleBuilderCache = new ConcurrentDictionary<string, Lazy<ModuleBuilder>>();

        public static Type ImplementType(Type type) =>
            type.IsInterface || type.IsAbstract
                ? typesCache.GetOrAdd(type, t => new Lazy<Type>(() => ImplementTypeInternal(t))).Value
                : type;

        public static Type ImplementWrapperType(Type type) =>
            type.IsInterface || type.IsClass
                ? wrappersCache.GetOrAdd(ImplementType(type), t => new Lazy<Type>(() => ImplementWrapperTypeInternal(t))).Value
                : throw new ArgumentException($"Can't implement wrapper type of not class type {type.FullName}");

        public static void SetCurrentInstance(object wrapper, object instance)
        {
            var field = wrapper.GetType().GetField(CurrentInstanceFieldName);
            if (field == null) throw new ArgumentException($"Type of given wrapper object {wrapper.GetType().FullName} doesn't have {CurrentInstanceFieldName} field");
            field.SetValue(wrapper, instance);
        }

        private static Type ImplementTypeInternal(Type baseType)
        {
            var typeBuilder = StartType(baseType, "Implementation");

            var properties = baseType
                .GetProperties()
                .Concat(baseType.GetInterfaces().SelectMany(iface => iface.GetProperties()))
                .Where(PropertyInfoExtensions.IsAbstract);

            foreach (var propertyInfo in properties)
                ImplementAutoProperty(typeBuilder, propertyInfo);

            ImplementDummyMethods(typeBuilder, baseType);
            return typeBuilder.CreateTypeInfo();
        }

        private static Type ImplementWrapperTypeInternal(Type baseType)
        {
            var typeBuilder = StartType(baseType, "Wrapper");
            var currentInstanceField = typeBuilder.DefineField(CurrentInstanceFieldName, baseType, FieldAttributes.Public);

            foreach (var propertyInfo in baseType.GetProperties().Where(PropertyInfoExtensions.IsVirtual))
                ImplementWrapperProperty(typeBuilder, propertyInfo, currentInstanceField);

            ImplementDummyMethods(typeBuilder, baseType);
            return typeBuilder.CreateTypeInfo();
        }

        private static TypeBuilder StartType(Type baseType, string suffix)
        {
            var builder = ObtainModuleBuilder(baseType);
            var genericSuffix = GetGenericArgsSuffix(baseType);
            var typeBuilder = builder.DefineType(
                $"{baseType.Namespace}.{baseType.Name}<{suffix}>{genericSuffix}",
                DefaultTypeAttributes,
                baseType.IsInterface ? null : baseType,
                baseType.IsInterface ? new[] {baseType} : Type.EmptyTypes);

            foreach (var attribute in baseType.CustomAttributes)
                typeBuilder.SetCustomAttribute(CreateCustomAttributeBuilder(attribute));

            return typeBuilder;
        }

        private static string GetGenericArgsSuffix(Type baseType)
        {
            if (!baseType.IsGenericType)
                return string.Empty;

            IStructuralEquatable genericArguments = baseType.GetGenericArguments();
            var hashCode = genericArguments.GetHashCode(EqualityComparer<Type>.Default);
            return $"<Generics{hashCode}>";
        }

        private static CustomAttributeBuilder CreateCustomAttributeBuilder(CustomAttributeData attribute)
        {
            var ctorArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
            if (attribute.NamedArguments == null || !attribute.NamedArguments.Any())
                return new CustomAttributeBuilder(attribute.Constructor, ctorArgs);

            var propInfos = new List<PropertyInfo>();
            var propValues = new List<object>();
            var fieldInfos = new List<FieldInfo>();
            var fieldValues = new List<object>();

            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.IsField)
                {
                    fieldInfos.Add((FieldInfo)argument.MemberInfo);
                    fieldValues.Add(argument.TypedValue.Value);
                }
                else
                {
                    propInfos.Add((PropertyInfo)argument.MemberInfo);
                    propValues.Add(argument.TypedValue.Value);
                }
            }

            return new CustomAttributeBuilder(
                attribute.Constructor,
                ctorArgs,
                propInfos.ToArray(),
                propValues.ToArray(),
                fieldInfos.ToArray(),
                fieldValues.ToArray());
        }

        private static ModuleBuilder ObtainModuleBuilder(Type type)
        {
            var key = type.IsPublic ? Default : type.Assembly.GetName().Name;

            return moduleBuilderCache.GetOrAdd(
                    key,
                    targetAssembly => new Lazy<ModuleBuilder>(
                        () => CreateModuleBuilder(targetAssembly)))
                .Value;
        }

        private static ModuleBuilder CreateModuleBuilder(string targetAssembly)
        {
            var isDefault = targetAssembly == Default;

            var name = isDefault
                ? DynamicAssemblyName
                : DynamicAssemblyName + "." + targetAssembly;

            var assemblyName = new AssemblyName(name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);

            if (!isDefault)
                SkipVisibilityChecksHelper.Setup(assemblyBuilder, moduleBuilder, new[] {targetAssembly});

            return moduleBuilder;
        }

        private static void ImplementAutoProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo) =>
            ImplementProperty(
                typeBuilder,
                propertyInfo,
                DefineBackingField(typeBuilder, propertyInfo),
                (gen, field) =>
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.Emit(OpCodes.Ret);
                },
                (gen, field) =>
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Stfld, field);
                    gen.Emit(OpCodes.Ret);
                });

        private static FieldInfo DefineBackingField(TypeBuilder typeBuilder, PropertyInfo propertyInfo) =>
            typeBuilder.DefineField($"<{propertyInfo.Name}>k__BackingField", propertyInfo.PropertyType, FieldAttributes.Private);

        private static void ImplementWrapperProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldInfo currentInstanceField) =>
            ImplementProperty(
                typeBuilder,
                propertyInfo,
                currentInstanceField,
                (gen, field) =>
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.Emit(OpCodes.Callvirt, propertyInfo.GetMethod);
                    gen.Emit(OpCodes.Ret);
                },
                (gen, field) =>
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Callvirt, propertyInfo.SetMethod);
                    gen.Emit(OpCodes.Ret);
                });

        private static void ImplementProperty(
            TypeBuilder typeBuilder,
            PropertyInfo propertyInfo,
            FieldInfo field,
            Action<ILGenerator, FieldInfo> createGetterBody,
            Action<ILGenerator, FieldInfo> createSetterBody)
        {
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, DefaultMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
            createGetterBody(getMethodBuilder.GetILGenerator(), field);

            var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, DefaultMethodAttributes, null, new[] {propertyInfo.PropertyType});
            createSetterBody(setMethodBuilder.GetILGenerator(), field);

            var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            OverrideGetterOrSetter(typeBuilder, propertyInfo.GetMethod, getMethodBuilder);
            OverrideGetterOrSetter(typeBuilder, propertyInfo.SetMethod, setMethodBuilder);

            foreach (var builder in propertyInfo.CustomAttributes.Select(CreateCustomAttributeBuilder))
                propertyBuilder.SetCustomAttribute(builder);
        }

        private static void OverrideGetterOrSetter(TypeBuilder typeBuilder, MethodInfo getterOrSetter, MethodBuilder getterOrSetterBuilder)
        {
            if (getterOrSetter == null) return;
            typeBuilder.DefineMethodOverride(getterOrSetterBuilder, getterOrSetter);
            foreach (var builder in getterOrSetter.CustomAttributes.Select(CreateCustomAttributeBuilder))
                getterOrSetterBuilder.SetCustomAttribute(builder);
        }

        private static void ImplementDummyMethods(TypeBuilder typeBuilder, Type type)
        {
            if (!type.IsAbstract) return;
            var notImplExceptionCtor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes);

            var methods = type
                .GetMethods()
                .Concat(type.GetInterfaces().SelectMany(iface => iface.GetMethods()))
                .Where(mi => mi.IsAbstract && !mi.IsSpecialName);

            foreach (var methodInfo in methods)
                ImplementDummyMethod(typeBuilder, methodInfo, notImplExceptionCtor);
        }

        private static void ImplementDummyMethod(TypeBuilder typeBuilder, MethodInfo methodInfo, ConstructorInfo notImplExceptionCtor)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                methodInfo.Name,
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual,
                methodInfo.ReturnType,
                methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());
            var ilGen = methodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, notImplExceptionCtor);
            ilGen.Emit(OpCodes.Throw);
            typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }
    }
}