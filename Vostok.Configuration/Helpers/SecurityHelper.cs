using System.Reflection;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.Attributes;

namespace Vostok.Configuration.Helpers
{
    [PublicAPI]
    public static class SecurityHelper
    {
        public static bool IsSecret([NotNull] MemberInfo member)
            => member.GetCustomAttribute<SecretAttribute>() != null;
    }
}