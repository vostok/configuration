using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration
{
    internal interface IConfigurationWithErrorsObservable
    {
        [NotNull]
        IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>();

        [NotNull]
        IObservable<(TSettings settings, Exception error)> ObserveWithErrors<TSettings>([NotNull] IConfigurationSource source);
    }
}