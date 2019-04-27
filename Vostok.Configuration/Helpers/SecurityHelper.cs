using System;
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

        public static bool IsSecret([NotNull] MemberInfo member)
            => IsInSecureScope || member.GetCustomAttribute<SecretAttribute>() != null;

        public static bool IsSecret([NotNull] Type type)
            => IsInSecureScope || type.GetCustomAttribute<SecretAttribute>() != null;

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
