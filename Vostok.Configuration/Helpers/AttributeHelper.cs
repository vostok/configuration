using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Commons.Collections;

namespace Vostok.Configuration.Helpers
{
    internal static class AttributeHelper
    {
        private const int CacheCapacity = 1000;

        private static readonly RecyclingBoundedCache<(Type type, Type attributeType), IEnumerable<Attribute>> TypeCache
            = new RecyclingBoundedCache<(Type type, Type attributeType), IEnumerable<Attribute>>(CacheCapacity);

        private static readonly RecyclingBoundedCache<(MemberInfo member, Type attributeType), IEnumerable<Attribute>> MemberCache
            = new RecyclingBoundedCache<(MemberInfo member, Type attributeType), IEnumerable<Attribute>>(CacheCapacity);

        [CanBeNull]
        public static TAttribute Get<TAttribute>(Type type)
            where TAttribute : Attribute
            => (TAttribute)GetAttributes(type, typeof(TAttribute)).FirstOrDefault();

        [CanBeNull]
        public static TAttribute Get<TAttribute>(MemberInfo member)
            where TAttribute : Attribute
            => (TAttribute)GetAttributes(member, typeof(TAttribute)).FirstOrDefault();

        [NotNull, ItemNotNull]
        public static TAttribute[] Select<TAttribute>(Type type)
            where TAttribute : Attribute
            => GetAttributes(type, typeof(TAttribute)).Cast<TAttribute>().ToArray();

        [NotNull, ItemNotNull]
        public static TAttribute[] Select<TAttribute>(MemberInfo member)
            where TAttribute : Attribute
            => GetAttributes(member, typeof(TAttribute)).Cast<TAttribute>().ToArray();

        public static bool Has<TAttribute>(Type type)
            where TAttribute : Attribute
            => Has(type, typeof(TAttribute));

        public static bool Has<TAttribute>(MemberInfo member)
            where TAttribute : Attribute
            => Has(member, typeof(TAttribute));

        public static bool Has(Type type, Type attributeType)
            => GetAttributes(type, attributeType).Any();

        public static bool Has(MemberInfo member, Type attributeType)
            => GetAttributes(member, attributeType).Any();

        [NotNull, ItemCanBeNull]
        private static IEnumerable<Attribute> GetAttributes(Type type, Type attributeType)
            => TypeCache.Obtain((type, attributeType), tuple => tuple.type.GetCustomAttributes(tuple.attributeType));

        [NotNull, ItemCanBeNull]
        private static IEnumerable<Attribute> GetAttributes(MemberInfo member, Type attributeType)
            => MemberCache.Obtain((member, attributeType), tuple => tuple.member.GetCustomAttributes(tuple.attributeType));
    }
}
