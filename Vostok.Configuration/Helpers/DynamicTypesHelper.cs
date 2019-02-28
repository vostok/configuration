using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vostok.Configuration.Binders.Extensions;

namespace Vostok.Configuration.Helpers
{
    internal static class DynamicTypesHelper
    {
        private static readonly ConcurrentDictionary<Type, Lazy<Type>> typesCache = new ConcurrentDictionary<Type, Lazy<Type>>();
        private static readonly object moduleBuilderSync = new object();
        private static volatile ModuleBuilder moduleBuilder;

        private static ModuleBuilder ModuleBuilder
        {
            get
            {
                if (moduleBuilder == null)
                {
                    lock (moduleBuilderSync)
                    {
                        if (moduleBuilder == null)
                        {
                            const string name = "Vostok.Configuration.Dynamic.dll";
                            var assemblyName = new AssemblyName(name);
                            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
                            moduleBuilder = assemblyBuilder.DefineDynamicModule(name);
                        }
                    }
                }

                return moduleBuilder;
            }
        }

        public static Type ImplementTypeIfNeeded(Type type) =>
            type.IsInterface || type.IsAbstract
                ? typesCache.GetOrAdd(type, t => new Lazy<Type>(() => ImplementType(t))).Value
                : type;

        private static Type ImplementType(Type baseType)
        {
            var name = $"{baseType.Namespace}.{baseType.Name}__Implementation";
            const TypeAttributes typeAttributes =
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.AutoLayout;

            var typeBuilder = ModuleBuilder.DefineType(
                name,
                typeAttributes,
                baseType.IsInterface ? null : baseType,
                baseType.IsInterface ? new[] {baseType} : Type.EmptyTypes);

            foreach (var propertyInfo in baseType.GetProperties().Where(PropertyInfoExtensions.IsAbstract))
                ImplementAutoProperty(typeBuilder, propertyInfo);

            var notImplExceptionCtor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes);
            foreach (var methodInfo in baseType.GetMethods().Where(mi => mi.IsAbstract && !mi.IsSpecialName))
                ImplementDummyMethod(typeBuilder, methodInfo, notImplExceptionCtor);

            return typeBuilder.CreateTypeInfo();
        }

        private static void ImplementAutoProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            var backingField = typeBuilder.DefineField($"<{propertyInfo.Name}>k__BackingField", propertyInfo.PropertyType, FieldAttributes.Private);

            const MethodAttributes methodAttributes =
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual |
                MethodAttributes.SpecialName;

            var getMethodBuilder = typeBuilder.DefineMethod(
                "get_" + propertyInfo.Name,
                methodAttributes,
                propertyInfo.PropertyType,
                Type.EmptyTypes);
            var getIlGen = getMethodBuilder.GetILGenerator();
            getIlGen.Emit(OpCodes.Ldarg_0);
            getIlGen.Emit(OpCodes.Ldfld, backingField);
            getIlGen.Emit(OpCodes.Ret);

            var setMethodBuilder = typeBuilder.DefineMethod(
                "set_" + propertyInfo.Name,
                methodAttributes,
                null,
                new[] {propertyInfo.PropertyType});
            var setIlGen = setMethodBuilder.GetILGenerator();
            setIlGen.Emit(OpCodes.Ldarg_0);
            setIlGen.Emit(OpCodes.Ldarg_1);
            setIlGen.Emit(OpCodes.Stfld, backingField);
            setIlGen.Emit(OpCodes.Ret);

            var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            if (propertyInfo.GetMethod != null)
                typeBuilder.DefineMethodOverride(getMethodBuilder, propertyInfo.GetMethod);
            if (propertyInfo.SetMethod != null)
                typeBuilder.DefineMethodOverride(setMethodBuilder, propertyInfo.SetMethod);
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