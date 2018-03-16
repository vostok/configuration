using System;

namespace Vostok.Configuration.Sources
{
    // TODO(krait): Implement ScopedSource: a source that takes RawSettings from a provided source, then walks down the tree by keys provided in the 'scope' parameter, and returns only the resulting part of the tree.
    public class ScopedSource : IConfigurationSource
    {
        public ScopedSource(IConfigurationSource source, params string[] scope)
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