using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.ProviderComponents
{
    internal interface IConfigurationGetter
    {
        [NotNull]
        TSettings Get<TSettings>();

        [NotNull]
        TSettings Get<TSettings>([NotNull] IConfigurationSource source);
    }
}