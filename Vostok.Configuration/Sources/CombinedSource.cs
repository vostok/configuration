using System;

namespace Vostok.Configuration.Sources
{
    // TODO(krait): Implement CombinedSource: a source that combines settings from several other sources, resolving possible conflicts in favor of sources that come earlier in the list.
    public class CombinedSource : IConfigurationSource
    {
        public CombinedSource(params IConfigurationSource[] sources)
        {
            throw new NotImplementedException();
        }

        public RawSettings Get()
        {
            throw new NotImplementedException();
        }

        public IObservable<RawSettings> Observe()
        {
            throw new NotImplementedException();
        }
    }
}