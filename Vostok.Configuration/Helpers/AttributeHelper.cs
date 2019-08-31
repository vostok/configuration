using System;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Commons.Collections;

namespace Vostok.Configuration.Helpers
{
    internal static class AttributeHelper
    {
        private const int CacheCapacity = 1000;

        private static readonly RecyclingBoundedCache<(Type type, Type attributeType), object> TypeCache
            = new RecyclingBoundedCache<(Type type, Type attributeType), object>(CacheCapacity);

        private static readonly RecyclingBoundedCache<(MemberInfo member, Type attributeType), object> MemberCache
            = new RecyclingBoundedCache<(MemberInfo member, Type attributeType), object>(CacheCapacity);

        [CanBeNull]
        public static TAttribute Get<TAttribute>(Type type)
            where TAttribute : Attribute
            => (TAttribute)GetAttribute(type, typeof(TAttribute));

        [CanBeNull]
        public static TAttribute Get<TAttribute>(MemberInfo member)
            where TAttribute : Attribute
            => (TAttribute)GetAttribute(member, typeof(TAttribute));

        public static bool Has<TAttribute>(Type type)
            where TAttribute : Attribute
            => Get<TAttribute>(type) != null;

        public static bool Has<TAttribute>(MemberInfo member)
            where TAttribute : Attribute
            => Get<TAttribute>(member) != null;

        public static bool Has(Type type, Type attributeType)
            => GetAttribute(type, attributeType) != null;

        public static bool Has(MemberInfo member, Type attributeType)
            => GetAttribute(member, attributeType) != null;

        [CanBeNull]
        private static object GetAttribute(Type type, Type attributeType)
            => TypeCache.Obtain((type, attributeType), tuple => tuple.type.GetCustomAttribute(tuple.attributeType));

        [CanBeNull]
        private static object GetAttribute(MemberInfo member, Type attributeType)
            => MemberCache.Obtain((member, attributeType), tuple => tuple.member.GetCustomAttribute(tuple.attributeType));
    }
}
