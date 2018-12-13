using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    internal interface IConfigurationGetter
    {
        [NotNull]
        TSettings Get<TSettings>();

        [NotNull]
        TSettings Get<TSettings>([NotNull] IConfigurationSource source);
    }
}