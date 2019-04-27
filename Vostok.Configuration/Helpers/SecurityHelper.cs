using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.Attributes;

namespace Vostok.Configuration.Helpers
{
    [PublicAPI]
    public static class SecurityHelper
    {
        [ThreadStatic]
        private static bool IsInSecureScope;

        private static volatile Type[] SecretAttributes = { typeof(SecretAttribute) };

        public static bool IsSecret([NotNull] MemberInfo member)
            => IsInSecureScope || SecretAttributes.Any(attr => member.GetCustomAttribute(attr) != null);

        public static bool IsSecret([NotNull] Type type)
            => IsInSecureScope || SecretAttributes.Any(attr => type.GetCustomAttribute(attr) != null);

        public static void RegisterCustomSecretAttribute<TAttribute>()
            => RegisterCustomSecretAttribute(typeof(TAttribute));

        public static void RegisterCustomSecretAttribute(Type attributeType)
            => SecretAttributes = SecretAttributes.Concat(new[] {attributeType}).ToArray();

        internal static IDisposable StartSecurityScope([NotNull] Type type)
            => IsSecret(type) ? new SecurityScopeToken() : new NoOpScopeToken() as IDisposable;

        internal static IDisposable StartSecurityScope([NotNull] MemberInfo member)
            => IsSecret(member) ? new SecurityScopeToken() : new NoOpScopeToken() as IDisposable;

        private class SecurityScopeToken : IDisposable
        {
            public SecurityScopeToken()
                => IsInSecureScope = true;

            public void Dispose()
                => IsInSecureScope = false;
        }

        private class NoOpScopeToken : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
