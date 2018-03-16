using System;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    /// <summary>
    /// In tests you substitute this one.
    /// Using a per-project extension method you can get rid of generic type on Get.
    /// </summary>
    public interface IConfigurationProvider
    {
        // TODO(krait): ICP decides whether to throw on invalid configs or ignore errors

        TSettings Get<TSettings>();

        // TODO(krait): take ISettings?
        TSettings Get<TSettings>(IConfigurationSource source);

        IObservable<TSettings> Observe<TSettings>();

        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
}