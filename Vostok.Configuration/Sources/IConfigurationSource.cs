using System;

namespace Vostok.Configuration.Sources
{
    public interface IConfigurationSource
    {
        RawSettings Get();

        IObservable<RawSettings> Observe();
    }
}