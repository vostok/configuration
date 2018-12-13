using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    internal interface IConfigurationObservable
    {
        [NotNull]
        IObservable<TSettings> Observe<TSettings>();

        [NotNull]
        IObservable<TSettings> Observe<TSettings>([NotNull] IConfigurationSource source);
    }
}