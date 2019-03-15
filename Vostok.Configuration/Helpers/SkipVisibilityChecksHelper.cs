using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Vostok.Configuration.Helpers
{
    /// <summary>
    /// https://github.com/Microsoft/vs-mef/blob/master/src/Microsoft.VisualStudio.Composition/Reflection/SkipClrVisibilityChecks.cs
    /// </summary>
    internal static class SkipVisibilityChecksHelper
    {
        private const string AttributeName = "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute";

        private static readonly ConstructorInfo AttributeBaseClassCtor = typeof(Attribute).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single(ctor => ctor.GetParameters().Length == 0);

        private static readonly ConstructorInfo AttributeUsageCtor = typeof(AttributeUsageAttribute).GetConstructor(new[] {typeof(AttributeTargets)});

        private static readonly PropertyInfo AttributeUsageAllowMultipleProperty = typeof(AttributeUsageAttribute).GetProperty(nameof(AttributeUsageAttribute.AllowMultiple));

        public static void Setup(AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder, string[] assemblies)
        {
            if (assemblyBuilder.GetType(AttributeName) != null)
                throw new InvalidOperationException("SkipVisibilityChecks hack has been already applied to this assembly");

            var attributeCtor = EmitMagicAttribute(moduleBuilder).GetConstructor(new[] { typeof(string) });
            
            foreach (var assembly in assemblies)
                SkipVisibilityChecksFor(assemblyBuilder, attributeCtor, assembly);
        }

        private static TypeInfo EmitMagicAttribute(ModuleBuilder moduleBuilder)
        {
            var tb = moduleBuilder.DefineType(
                AttributeName,
                TypeAttributes.NotPublic,
                typeof(Attribute));

            var attributeUsage = new CustomAttributeBuilder(
                AttributeUsageCtor,
                new object[] {AttributeTargets.Assembly},
                new[] {AttributeUsageAllowMultipleProperty},
                new object[] {true});
            tb.SetCustomAttribute(attributeUsage);

            var cb = tb.DefineConstructor(
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] {typeof(string)});
            cb.DefineParameter(1, ParameterAttributes.None, "assemblyName");

            var il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, AttributeBaseClassCtor);
            il.Emit(OpCodes.Ret);

            return tb.CreateTypeInfo();
        }

        private static void SkipVisibilityChecksFor(AssemblyBuilder assemblyBuilder, ConstructorInfo constructorInfo, string assemblyName)
        {
            var assemblyNameArg = assemblyName;
            var cab = new CustomAttributeBuilder(constructorInfo, new object[] {assemblyNameArg});
            assemblyBuilder.SetCustomAttribute(cab);
        }
    }
}