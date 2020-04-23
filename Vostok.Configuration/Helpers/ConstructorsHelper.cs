using System;
using System.Collections.Generic;
using System.Reflection;
using Vostok.Commons.Collections;

namespace Vostok.Configuration.Helpers
{
    internal static class ConstructorsHelper
    {
        private const int CacheCapacity = 1000;

        private static readonly RecyclingBoundedCache<Type, ConstructorInfo[]> Constructors = 
            new RecyclingBoundedCache<Type, ConstructorInfo[]>(CacheCapacity);

        public static IReadOnlyList<ConstructorInfo> GetConstructors(Type type) =>
            Constructors.Obtain(type, t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
    }
}